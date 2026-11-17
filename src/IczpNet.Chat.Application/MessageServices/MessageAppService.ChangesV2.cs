using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.MessageServices;

public partial class MessageAppService
{
    private CancellationToken ChangeCancellationToken =>
        LazyServiceProvider.LazyGetRequiredService<ICancellationTokenProvider>().Token;

    private MessageOptions GetV2Options()
    {
        var options = LazyServiceProvider.LazyGetRequiredService<IOptions<MessageOptions>>().Value;
        if (!options.ChangeSyncEnabled || !options.ChangeSyncV2Enabled)
            throw new EntityNotFoundException("Message Change Sync V2 is disabled.");
        return options;
    }

    private static string ToSeq(long version) => version.ToString(CultureInfo.InvariantCulture);

    private static string GetV2Epoch(MessageChangeWriteGateState gate, MessageOptions options)
    {
        var value = $"Chat.MessageChanges.v2:{gate.Epoch:D}:{options.ChangeSyncV2Generation}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    [Authorize]
    [HttpPost("/api/chat/message/changes/head")]
    [UnitOfWork(isTransactional: false)]
    public async Task<MessageChangeHeadV2Dto> GetChangeHeadAsync(MessageChangeHeadInput input)
    {
        var options = GetV2Options();
        var userId = CurrentUser.GetId();
        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));
        var token = ChangeCancellationToken;
        var gate = await ChangeRepository.GetWriteGateAsync(token);
        var cursor = await ChangeRepository.GetCursorAsync(CurrentTenant.Id, userId, input.ChatObjectId, input.DeviceId.Trim(), token);
        return new MessageChangeHeadV2Dto(
            GetV2Epoch(gate, options),
            ToSeq(gate.LastCommittedChangeId),
            ToSeq(cursor?.LastAppliedMessageChangeId ?? 0),
            cursor?.IsInitialized ?? false);
    }

    [Authorize]
    [HttpPost("/api/chat/message/changes/pull")]
    [UnitOfWork(isTransactional: false)]
    public async Task<MessageChangePullDto> PullChangesAsync(MessageChangePullInput input)
    {
        var options = GetV2Options();
        if (input.MaxResultCount is < 1 or > 200 ||
            !long.TryParse(input.AfterMessageChangeId, NumberStyles.None, CultureInfo.InvariantCulture, out var after) || after < 0)
            throw new UserFriendlyException("消息同步游标或批次大小无效。");

        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));
        var token = ChangeCancellationToken;
        var gate = await ChangeRepository.GetWriteGateAsync(token);
        var epoch = GetV2Epoch(gate, options);
        if (!string.Equals(input.Epoch, epoch, StringComparison.OrdinalIgnoreCase) || after > gate.LastCommittedChangeId)
        {
            return new MessageChangePullDto(
                epoch,
                ToSeq(gate.LastCommittedChangeId),
                input.AfterMessageChangeId,
                HasMore: false,
                ResetRequired: true,
                Items: []);
        }

        var page = await ChangeRepository.PullChangesAsync(
            input.ChatObjectId,
            after,
            gate.LastCommittedChangeId,
            input.MaxResultCount,
            token);

        var verified = await ChangeRepository.GetWriteGateAsync(token);
        var verifiedEpoch = GetV2Epoch(verified, options);
        if (!string.Equals(verifiedEpoch, epoch, StringComparison.OrdinalIgnoreCase) ||
            verified.LastCommittedChangeId < gate.LastCommittedChangeId)
        {
            return new MessageChangePullDto(
                verifiedEpoch,
                ToSeq(verified.LastCommittedChangeId),
                input.AfterMessageChangeId,
                HasMore: false,
                ResetRequired: true,
                Items: []);
        }

        var items = page.Items.Select(row => new MessageChangePullItemDto(
            ToSeq(row.MessageChangeId),
            row.SessionId,
            row.SessionUnitId,
            ToSeq(row.MessageId),
            row.ChangeType switch
            {
                MessageChangeKind.Update => "Updated",
                MessageChangeKind.Rollback => "Recalled",
                _ => throw new InvalidOperationException($"Unsupported shared message change kind: {row.ChangeType}.")
            })).ToList();

        return new MessageChangePullDto(
            epoch,
            ToSeq(gate.LastCommittedChangeId),
            ToSeq(page.NextCursor),
            page.HasMore,
            ResetRequired: false,
            Items: items);
    }

    [Authorize]
    [HttpPost("/api/chat/message/changes/ack")]
    [UnitOfWork(isTransactional: true)]
    public async Task<MessageChangeAckDto> AckChangesAsync(MessageChangeAckInput input)
    {
        var options = GetV2Options();
        if (!long.TryParse(input.LastAppliedMessageChangeId, NumberStyles.None, CultureInfo.InvariantCulture, out var ackId) || ackId < 0)
            throw new UserFriendlyException("消息同步确认游标无效。");

        var userId = CurrentUser.GetId();
        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));
        var token = ChangeCancellationToken;
        var gate = await ChangeRepository.GetWriteGateAsync(token);
        var epoch = GetV2Epoch(gate, options);
        if (!string.Equals(input.Epoch, epoch, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await ChangeRepository.GetCursorAsync(CurrentTenant.Id, userId, input.ChatObjectId, input.DeviceId.Trim(), token);
            return new MessageChangeAckDto(
                epoch,
                ToSeq(gate.LastCommittedChangeId),
                ToSeq(existing?.LastAppliedMessageChangeId ?? 0),
                existing?.IsInitialized ?? false,
                ResetRequired: true);
        }

        if (ackId > gate.LastCommittedChangeId)
            throw new UserFriendlyException("确认游标不能超过服务端当前已提交的变更位置。");

        var cursor = await ChangeRepository.UpsertCursorAsync(
            CurrentTenant.Id,
            userId,
            input.ChatObjectId,
            input.DeviceId.Trim(),
            ackId,
            Clock.Now,
            input.Reset,
            token);

        return new MessageChangeAckDto(
            epoch,
            ToSeq(gate.LastCommittedChangeId),
            ToSeq(cursor.LastAppliedMessageChangeId),
            cursor.IsInitialized,
            ResetRequired: false);
    }

    [Authorize]
    [HttpPost("/api/chat/message/by-ids")]
    [UnitOfWork(isTransactional: false)]
    public async Task<MessageByIdsDto> GetByIdsAsync(MessageByIdsInput input)
    {
        GetV2Options();
        var scopesInput = ResolveByIdsRequests(input);
        if (scopesInput.Count == 0 || scopesInput.Sum(x => x.MessageIds.Count) > 500)
            throw new UserFriendlyException("每批需要有效的会话和消息 ID（总计不超过 500 条）。");

        var token = ChangeCancellationToken;
        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));

        var unitIds = scopesInput.Select(x => x.SessionUnitId).Distinct().ToList();
        var scopes = (await ChangeRepository.GetV2ScopesAsync(input.ChatObjectId, unitIds, token))
            .ToDictionary(x => x.UnitId);

        var results = new List<MessageByIdDto>();
        foreach (var req in scopesInput)
        {
            var ids = req.MessageIds.Distinct().ToList();
            if (!scopes.TryGetValue(req.SessionUnitId, out var scope))
            {
                foreach (var id in ids)
                    results.Add(new MessageByIdDto(req.SessionUnitId, ToSeq(id), "NotAccessible", null, null, null, null, null));
                continue;
            }

            var states = (await ChangeRepository.ReadV2ItemsAsync(scope, ids, token)).ToDictionary(x => x.Message.Id);
            var unresolved = ids.Where(id => !states.ContainsKey(id)).ToList();
            var softDeleted = unresolved.Count == 0 ? [] :
                (await ChangeRepository.ReadV2SoftDeletedIdsAsync(scope, unresolved, token)).ToHashSet();
            var bodies = (await MapV2MessagesAsync(states.Values
                .Where(x => !x.IsDeleted && !x.Message.IsRollbacked && !x.Message.RollbackTime.HasValue)
                .Select(x => x.Message).ToList(), token)).ToDictionary(x => x.Id);

            foreach (var id in ids)
            {
                if (!states.TryGetValue(id, out var state))
                {
                    results.Add(new MessageByIdDto(
                        req.SessionUnitId,
                        ToSeq(id),
                        softDeleted.Contains(id) ? "NotFound" : "NotAccessible",
                        null,
                        null,
                        null,
                        null,
                        null));
                    continue;
                }

                var message = state.Message;
                var msgChangeId = message.MessageChangeId.HasValue ? ToSeq(message.MessageChangeId.Value) : null;
                if (state.IsDeleted)
                {
                    results.Add(new MessageByIdDto(
                        req.SessionUnitId,
                        ToSeq(id),
                        "DeletedForMe",
                        msgChangeId,
                        message.EntityVersion,
                        state.DeleteChangeSeq.HasValue ? ToSeq(state.DeleteChangeSeq.Value) : null,
                        null,
                        null));
                    continue;
                }

                var seq = message.ChangeSeq.HasValue ? ToSeq(message.ChangeSeq.Value) : null;
                if (message.IsRollbacked || message.RollbackTime.HasValue)
                {
                    results.Add(new MessageByIdDto(
                        req.SessionUnitId,
                        ToSeq(id),
                        "Recalled",
                        msgChangeId,
                        message.EntityVersion,
                        seq,
                        null,
                        message.RollbackTime));
                    continue;
                }

                results.Add(new MessageByIdDto(
                    req.SessionUnitId,
                    ToSeq(id),
                    "Visible",
                    msgChangeId,
                    message.EntityVersion,
                    seq,
                    bodies[id],
                    null));
            }
        }

        return new MessageByIdsDto(results);
    }

    private static List<MessageByIdsScopeRequest> ResolveByIdsRequests(MessageByIdsInput input)
    {
        if (input.Requests != null && input.Requests.Count > 0)
        {
            foreach (var r in input.Requests)
            {
                if (r.MessageIds == null || r.MessageIds.Count is < 1 or > 200 || r.MessageIds.Any(x => x <= 0))
                    throw new UserFriendlyException("每批需要 1～200 个有效消息 ID。");
            }
            return input.Requests;
        }

        if (input.SessionUnitId.HasValue && input.MessageIds != null && input.MessageIds.Count is >= 1 and <= 200 &&
            !input.MessageIds.Any(x => x <= 0))
        {
            return
            [
                new MessageByIdsScopeRequest
                {
                    SessionUnitId = input.SessionUnitId.Value,
                    MessageIds = input.MessageIds
                }
            ];
        }

        throw new UserFriendlyException("每批需要 1～200 个有效消息 ID。");
    }

    private async Task<ExtraPagedResultDto<MessageFastDto>> GetAuthoritativeWindowAsync(Guid sessionUnitId, int maxResultCount,
        long? minId, long? maxId, bool descending)
    {
        GetV2Options();
        if (maxResultCount is < 1 or > 200)
            throw new UserFriendlyException("消息同步请求超过批次限制。");
        var sessionUnit = await GetAndCheckPolicyAsync(GetListPolicyName, sessionUnitId);
        var token = ChangeCancellationToken;
        var scope = new MessageChangeUnitScope(sessionUnit.Id, sessionUnit.SessionId!.Value, sessionUnit.OwnerId, null);
        var messages = await ChangeRepository.ReadV2WindowAsync(scope, minId, maxId, maxResultCount + 1, descending, token);
        var count = messages.Count;
        var more = count > maxResultCount;
        if (more) messages.RemoveAt(messages.Count - 1);
        return new ExtraPagedResultDto<MessageFastDto>(count, await MapV2MessagesAsync(messages, token), new
        {
            Authoritative = true,
            HasMore = more,
            NextCursorId = more ? ToSeq(messages[^1].Id) : null
        });
    }

    private async Task<MessageChangeUnitScope> GetV2ScopeAsync(long ownerId, Guid unitId, CancellationToken token)
    {
        await CheckPolicyForUserAsync(ownerId, () => CheckPolicyAsync(GetListPolicyName));
        return (await ChangeRepository.GetV2ScopesAsync(ownerId, [unitId], token)).SingleOrDefault()
            ?? throw new AbpAuthorizationException("The requested session unit is unavailable.");
    }

    private async Task<List<MessageFastDto>> MapV2MessagesAsync(List<Message> messages, CancellationToken token)
    {
        if (messages.Count == 0) return [];
        var visibleIds = messages.Where(x => !x.IsRollbacked && !x.RollbackTime.HasValue).Select(x => x.Id).ToList();
        var cached = visibleIds.Count == 0 ? [] :
            (await MessageManager.GetManyCacheAsync(visibleIds, hideErrors: true, token: token))
            .Where(x => x.Value != null).ToDictionary(x => x.Key.MessageId, x => x.Value);
        var domainMapper = LazyServiceProvider.LazyGetRequiredService<IObjectMapper<ChatDomainModule>>();
        var authoritative = new List<MessageCacheItem>(messages.Count);
        foreach (var message in messages)
        {
            cached.TryGetValue(message.Id, out var item);
            if (item == null || item.ChangeSeq != message.ChangeSeq || item.SyncVersion != message.SyncVersion ||
                item.IsRollbacked != message.IsRollbacked || item.RollbackTime != message.RollbackTime)
            {
                var sqlItem = domainMapper.Map<Message, MessageCacheItem>(message);
                if (item?.SenderSessionUnit != null && sqlItem.SenderSessionUnit == null &&
                    item.SenderSessionUnitId == sqlItem.SenderSessionUnitId)
                    sqlItem.SenderSessionUnit = item.SenderSessionUnit;
                item = sqlItem;
            }
            authoritative.Add(item);
        }

        return authoritative.Select(item =>
        {
            var dto = MapToMessage(item);
            if (item.IsRollbacked || item.RollbackTime.HasValue)
            {
                dto.Content = null;
                dto.QuoteMessage = null;
            }
            return dto;
        }).ToList();
    }
}
