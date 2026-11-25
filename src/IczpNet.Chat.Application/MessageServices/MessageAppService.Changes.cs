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
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageServices;

public partial class MessageAppService
{
    private IMessageChangeRepository ChangeRepository =>
        LazyServiceProvider.LazyGetRequiredService<IMessageChangeRepository>();

    private CancellationToken ChangeCancellationToken =>
        LazyServiceProvider.LazyGetRequiredService<ICancellationTokenProvider>().Token;

    private MessageOptions GetChangeSyncOptions()
    {
        var options = LazyServiceProvider.LazyGetRequiredService<IOptions<MessageOptions>>().Value;
        if (!options.ChangeSyncEnabled)
            throw new EntityNotFoundException("Message Change Sync is disabled.");
        return options;
    }

    private static string ToSeq(long version) => version.ToString(CultureInfo.InvariantCulture);

    [Authorize]
    [HttpPost("/api/chat/message/by-ids")]
    [UnitOfWork(isTransactional: false)]
    public async Task<MessageByIdsDto> GetByIdsAsync(MessageByIdsInput input)
    {
        GetChangeSyncOptions();
        var scopesInput = ResolveByIdsRequests(input);
        if (scopesInput.Count == 0 || scopesInput.Sum(x => x.MessageIds.Count) > 500)
            throw new UserFriendlyException("每批需要有效的会话和消息 ID（总计不超过 500 条）。");

        var token = ChangeCancellationToken;
        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));

        var unitIds = scopesInput.Select(x => x.SessionUnitId).Distinct().ToList();
        var scopes = (await ChangeRepository.GetScopesAsync(input.ChatObjectId, unitIds, token))
            .ToDictionary(x => x.UnitId);

        var results = new List<MessageByIdDto>();
        foreach (var req in scopesInput)
        {
            var ids = req.MessageIds.Distinct().ToList();
            if (!scopes.TryGetValue(req.SessionUnitId, out var scope))
            {
                foreach (var id in ids)
                    results.Add(new MessageByIdDto(req.SessionUnitId, ToSeq(id), "NotAccessible", null, null, null, null));
                continue;
            }

            var states = (await ChangeRepository.ReadItemsAsync(scope, ids, token)).ToDictionary(x => x.Message.Id);
            var unresolved = ids.Where(id => !states.ContainsKey(id)).ToList();
            var softDeleted = unresolved.Count == 0 ? [] :
                (await ChangeRepository.ReadSoftDeletedIdsAsync(scope, unresolved, token)).ToHashSet();
            var bodies = (await MapAuthoritativeMessagesAsync(states.Values
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
                        null,
                        null));
                    continue;
                }

                if (message.IsRollbacked || message.RollbackTime.HasValue)
                {
                    results.Add(new MessageByIdDto(
                        req.SessionUnitId,
                        ToSeq(id),
                        "Recalled",
                        msgChangeId,
                        message.EntityVersion,
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
        long? minId, long? maxId, bool descending, bool compactSenders = false)
    {
        // Authoritative message reads remain available when change notifications
        // are disabled. Permission and member visibility are checked below.
        if (maxResultCount is < 1 or > 200)
            throw new UserFriendlyException("消息同步请求超过批次限制。");
        var sessionUnit = await GetAndCheckPolicyAsync(GetListPolicyName, sessionUnitId);
        var token = ChangeCancellationToken;
        var scope = new MessageChangeUnitScope(sessionUnit.Id, sessionUnit.SessionId!.Value, sessionUnit.OwnerId, null);
        var messages = await ChangeRepository.ReadWindowAsync(scope, minId, maxId, maxResultCount + 1, descending, token);
        var count = messages.Count;
        var more = count > maxResultCount;
        if (more) messages.RemoveAt(messages.Count - 1);
        var items = await MapAuthoritativeMessagesAsync(messages, token);
        var nextCursorId = more ? ToSeq(messages[^1].Id) : null;
        object extra = compactSenders
            ? new
            {
                Authoritative = true,
                HasMore = more,
                NextCursorId = nextCursorId,
                Senders = ExtractCompactSenders(items)
            }
            : new
            {
                Authoritative = true,
                HasMore = more,
                NextCursorId = nextCursorId
            };
        return new ExtraPagedResultDto<MessageFastDto>(count, items, extra);
    }

    private async Task<List<MessageFastDto>> MapAuthoritativeMessagesAsync(List<Message> messages, CancellationToken token)
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
            if (item == null || item.SyncVersion != message.SyncVersion ||
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

        // Batch hydrate missing sender snapshots for SQL fallback items using deduplicated SenderSessionUnitIds.
        var incompleteSenders = authoritative
            .SelectMany(x => x.QuoteMessage == null ? new MessageInfo[] { x } : [x, x.QuoteMessage])
            .Where(x => x.SenderSessionUnitId.HasValue && x.SenderSessionUnit?.Owner == null)
            .ToList();
        if (incompleteSenders.Count > 0)
        {
            var distinctUnitIds = incompleteSenders.Select(x => x.SenderSessionUnitId!.Value).Distinct().ToList();
            var units = await SessionUnitManager.GetCacheManyAsync(distinctUnitIds);
            var validUnits = units.Where(x => x.Value != null).Select(x => x.Value).ToList();
            if (validUnits.Count > 0)
            {
                var senderMap = (await SessionUnitManager.MapToSenderAsync(validUnits)).ToDictionary(x => x.Id);
                foreach (var entry in incompleteSenders)
                {
                    if (senderMap.TryGetValue(entry.SenderSessionUnitId!.Value, out var sender))
                    {
                        entry.SenderSessionUnit = sender;
                    }
                }
            }
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
