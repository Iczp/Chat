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

    private static string GetV2Epoch(MessageChangeCheckpoint checkpoint, Guid unitId, MessageOptions options)
    {
        var value = $"Chat.MessageChanges.v2:{checkpoint.Epoch:D}:{unitId:D}:{options.ChangeSyncV2Generation}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    [Authorize]
    [HttpPost("/api/chat/message/changes/heads")]
    [UnitOfWork(isTransactional: false)]
    public async Task<MessageChangeHeadsDto> GetChangeHeadsAsync(MessageChangeHeadsInput input)
    {
        var options = GetV2Options();
        if (input.SessionUnitIds == null || input.SessionUnitIds.Count is < 1 or > 200)
            throw new UserFriendlyException("消息同步请求超过批次限制。");
        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));
        var requested = input.SessionUnitIds.Distinct().ToList();
        var token = ChangeCancellationToken;
        var scopes = (await ChangeRepository.GetV2ScopesAsync(input.ChatObjectId, requested, token))
            .ToDictionary(x => x.UnitId);
        var heads = scopes.Count > 0
            ? await ChangeRepository.GetCheckpointsAsync(scopes.Values.Select(x => x.SessionId).Distinct().ToList(), token)
            : [];
        // The clear boundary used during initialization must be from a read
        // after the head. A clear committed after that head is also safe to replay.
        var scopedBoundaries = scopes.Count == 0 ? scopes :
            (await ChangeRepository.GetV2ScopesAsync(input.ChatObjectId, scopes.Keys.ToList(), token))
            .ToDictionary(x => x.UnitId);
        return new(requested.Select(id =>
        {
            if (!scopedBoundaries.TryGetValue(id, out var scope) || !heads.TryGetValue(scope.SessionId, out var head))
                return new MessageChangeHeadDto(id, null, null, null, null, "Unavailable");
            // The shared head exposes aggregate activity only. Private change
            // identities and content are scoped by the current session unit.
            return new MessageChangeHeadDto(id, GetV2Epoch(head, id, options), ToSeq(head.Version), "0", scope.ClearTime);
        }).ToList());
    }

    [Authorize]
    [HttpPost("/api/chat/message/changes/incremental")]
    [UnitOfWork(isTransactional: false)]
    public async Task<MessageChangeIncrementalDto> GetIncrementalChangesAsync(MessageChangeIncrementalInput input)
    {
        var options = GetV2Options();
        if (input.MaxResultCount is < 1 or > 200 ||
            !long.TryParse(input.ChangeSeq, NumberStyles.None, CultureInfo.InvariantCulture, out var after) || after < 0)
            throw new UserFriendlyException("消息同步游标或批次大小无效。");

        var token = ChangeCancellationToken;
        var scope = await GetV2ScopeAsync(input.ChatObjectId, input.SessionUnitId, token);
        var head = await ChangeRepository.GetCheckpointAsync(scope.SessionId, token);
        var epoch = GetV2Epoch(head, scope.UnitId, options);
        // V1 MinVersion is a log retention boundary. V2 reads latest
        // projections which remain available even when old log rows expire.
        if (input.Epoch != epoch || after > head.Version)
            return new(epoch, ToSeq(head.Version), "0", input.ChangeSeq, false, true, []);

        var rows = await ChangeRepository.ReadV2ChangesAsync(scope, after, head.Version, input.MaxResultCount + 1, token);
        var verified = await ChangeRepository.GetCheckpointAsync(scope.SessionId, token);
        var verifiedEpoch = GetV2Epoch(verified, scope.UnitId, options);
        if (verifiedEpoch != epoch || verified.Version < head.Version)
            return new(verifiedEpoch, ToSeq(verified.Version), "0", input.ChangeSeq, false, true, []);

        var hasMore = rows.Count > input.MaxResultCount;
        if (hasMore) rows.RemoveAt(rows.Count - 1);
        // All visible positions through the captured head were scanned when
        // there is no next row. A row moved to a higher version is picked up
        // after this head, even if it disappeared from the current page.
        var next = hasMore ? rows[^1].Version : head.Version;
        return new(epoch, ToSeq(head.Version), "0", ToSeq(next), hasMore, false, rows.Select(row =>
            new MessageChangeV2ItemDto(row.MessageId.HasValue ? ToSeq(row.MessageId.Value) : null, ToSeq(row.Version),
                row.Kind switch
                {
                    MessageChangeKind.Update => "Updated",
                    MessageChangeKind.Rollback => "Recalled",
                    MessageChangeKind.Delete => "DeletedForMe",
                    MessageChangeKind.Clear => "Clear",
                    _ => throw new InvalidOperationException("Unknown message change projection.")
                },
                row.Kind == MessageChangeKind.Clear ? row.Time : null,
                row.Kind == MessageChangeKind.Rollback ? row.Time : null)).ToList());
    }

    [Authorize]
    [HttpPost("/api/chat/message/by-ids")]
    [UnitOfWork(isTransactional: false)]
    public async Task<MessageByIdsDto> GetByIdsAsync(MessageByIdsInput input)
    {
        GetV2Options();
        if (input.MessageIds == null || input.MessageIds.Count is < 1 or > 200 || input.MessageIds.Any(x => x <= 0))
            throw new UserFriendlyException("每批需要 1～200 个有效消息 ID。");
        var token = ChangeCancellationToken;
        var scope = await GetV2ScopeAsync(input.ChatObjectId, input.SessionUnitId, token);
        var ids = input.MessageIds.Distinct().ToList();
        var states = (await ChangeRepository.ReadV2ItemsAsync(scope, ids, token)).ToDictionary(x => x.Message.Id);
        var unresolved = ids.Where(id => !states.ContainsKey(id)).ToList();
        var softDeleted = unresolved.Count == 0 ? [] :
            (await ChangeRepository.ReadV2SoftDeletedIdsAsync(scope, unresolved, token)).ToHashSet();
        var bodies = (await MapV2MessagesAsync(states.Values
            .Where(x => !x.IsDeleted && !x.Message.IsRollbacked && !x.Message.RollbackTime.HasValue)
            .Select(x => x.Message).ToList(), token)).ToDictionary(x => x.Id);

        return new(ids.Select(id =>
        {
            if (!states.TryGetValue(id, out var state))
                return new MessageByIdDto(ToSeq(id), softDeleted.Contains(id) ? "NotFound" : "NotAccessible", null, null, null);
            var message = state.Message;
            if (state.IsDeleted)
                return new MessageByIdDto(ToSeq(id), "DeletedForMe",
                    state.DeleteChangeSeq.HasValue ? ToSeq(state.DeleteChangeSeq.Value) : null, null, null);
            var seq = message.ChangeSeq.HasValue ? ToSeq(message.ChangeSeq.Value) : null;
            if (message.IsRollbacked || message.RollbackTime.HasValue)
                return new MessageByIdDto(ToSeq(id), "Recalled", seq, null, message.RollbackTime);
            return new MessageByIdDto(ToSeq(id), "Visible", seq, bodies[id], null);
        }).ToList());
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
