using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.Specifications;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace IczpNet.Chat.MessageServices;

public partial class MessageAppService
{
    private record ChangeCursor(Guid UnitId, Guid Epoch, long Version);
    private IDataProtector ChangeProtector => LazyServiceProvider.LazyGetRequiredService<IDataProtectionProvider>()
        .CreateProtector("Chat.MessageChanges.v1");
    private IMessageChangeRepository ChangeRepository => LazyServiceProvider.LazyGetRequiredService<IMessageChangeRepository>();
    private void CheckChangeSync()
    {
        if (!LazyServiceProvider.LazyGetRequiredService<IOptions<MessageOptions>>().Value.ChangeSyncEnabled)
            throw new BusinessException("Chat:MessageChangeSyncDisabled").WithData("Reason", "Deploy the message change schema before enabling MessageOptions:ChangeSyncEnabled.");
    }
    private string ProtectCursor(Guid unitId, MessageChangeCheckpoint head, long version) =>
        ChangeProtector.Protect(JsonSerializer.Serialize(new ChangeCursor(unitId, head.Epoch, version)));
    private ChangeCursor ReadCursor(string value)
    {
        try { return JsonSerializer.Deserialize<ChangeCursor>(ChangeProtector.Unprotect(value)) ?? throw new JsonException(); }
        catch (Exception ex) when (ex is CryptographicException or JsonException or FormatException)
        { return null; }
    }

    [HttpPost("/api/chat/message/changes/query")]
    [UnitOfWork(isTransactional: false)]
    public async Task<List<MessageChangePageDto>> QueryChangesAsync(MessageChangesInput input)
    {
        CheckChangeSync();
        if (input.Sessions == null || input.Sessions.Count is < 1 or > 20 || input.MaxResultCount is < 1 or > 200 || input.MaxResultCount < input.Sessions.Count)
            throw new UserFriendlyException("消息同步请求超过批次限制。");
        var token = LazyServiceProvider.LazyGetRequiredService<ICancellationTokenProvider>().Token;
        var cache = LazyServiceProvider.LazyGetRequiredService<IMessageCacheManager>();
        var results = new List<MessageChangePageDto>();
        var perScope = Math.Max(1, input.MaxResultCount / input.Sessions.Count);
        var authorized = new Dictionary<Guid, SessionUnit>();
        foreach (var scope in input.Sessions)
        {
            try { authorized[scope.SessionUnitId] = await GetAndCheckPolicyAsync(GetListPolicyName, scope.SessionUnitId, checkIsKilled: false); }
            catch (Exception ex) when (ex is AbpAuthorizationException or EntityNotFoundException)
            { results.Add(new MessageChangePageDto { SessionUnitId = scope.SessionUnitId, ErrorCode = "Unavailable" }); }
        }
        if (authorized.Count == 0) return results;
        var sessionIds = authorized.Values.Select(x => x.SessionId!.Value).Distinct().ToList();
        var heads = await ChangeRepository.GetCheckpointsAsync(sessionIds, token);
        var scannedFrom = new Dictionary<Guid, long>();
        foreach (var scope in input.Sessions)
        {
            if (!authorized.TryGetValue(scope.SessionUnitId, out var unit)) continue;
            var head = heads[unit.SessionId!.Value];
            var result = new MessageChangePageDto { SessionUnitId = unit.Id,
                NextCursor = ProtectCursor(unit.Id, head, head.Version),
                SnapshotCursor = ProtectCursor(unit.Id, head, head.Version) };
            results.Add(result);
            if (string.IsNullOrEmpty(scope.Cursor)) { result.ResetRequired = true; continue; }
            var cursor = ReadCursor(scope.Cursor);
            var upper = string.IsNullOrEmpty(scope.SnapshotCursor) ? head.Version : ReadCursor(scope.SnapshotCursor)?.Version ?? -1;
            var snapshot = string.IsNullOrEmpty(scope.SnapshotCursor) ? cursor : ReadCursor(scope.SnapshotCursor);
            if (cursor == null || snapshot == null || cursor.UnitId != unit.Id || cursor.Epoch != head.Epoch || cursor.Version < head.MinVersion ||
                cursor.Version > head.Version || upper < cursor.Version || upper > head.Version ||
                snapshot.UnitId != unit.Id || snapshot.Epoch != head.Epoch)
            { result.ResetRequired = true; continue; }
            result.SnapshotCursor = ProtectCursor(unit.Id, head, upper);
            scannedFrom[unit.Id] = cursor.Version;
            var entries = await cache.GetChangeWindowAsync(unit.SessionId.Value, head.Epoch, cursor.Version, upper,
                perScope, () => ChangeRepository.ReadChangesAsync(unit.SessionId.Value, cursor.Version, upper, perScope, token), token);
            var setting = await SessionUnitSettingManager.GetOrAddCacheAsync(unit.Id);
            var eligible = entries.Where(x => x.ChangeType == MessageChangeKind.Delete
                ? x.ReceiverSessionUnitId == unit.Id
                : OwnerMessageSpecification.IsVisible(new SessionMessageWindowCacheItem.Entry
            { Id = x.MessageId, CreationTime = x.CreationTime, IsPrivate = x.IsPrivate,
                SenderSessionUnitId = x.SenderSessionUnitId, ReceiverSessionUnitId = x.ReceiverSessionUnitId }, unit.Id, setting)).ToList();
            var deleted = (await DeletedRecorderManager.GetRecorderMessageIdListAsync(unit.Id, eligible.Select(x => x.MessageId).ToList())).ToHashSet();
            result.Items = eligible.Where(x => x.ChangeType == MessageChangeKind.Delete || !deleted.Contains(x.MessageId)).Select(x => new MessageSyncChangeDto
            { MessageId = x.MessageId, SyncVersion = x.Version, ChangeType = x.ChangeType,
                ChangedAt = x.ChangedAt, IsRollbacked = x.ChangeType == MessageChangeKind.Rollback,
                IsDeleted = x.ChangeType == MessageChangeKind.Delete,
                RollbackTime = x.ChangeType == MessageChangeKind.Rollback ? x.ChangedAt : null }).ToList();
            // Advance by scanned entries, including ones not visible to this member.
            var next = entries.Count == perScope ? entries[^1].Version : upper;
            result.HasMore = next < upper;
            result.NextCursor = ProtectCursor(unit.Id, head, next);
        }
        var verifiedHeads = await ChangeRepository.GetCheckpointsAsync(sessionIds, token);
        foreach (var result in results.Where(x => scannedFrom.ContainsKey(x.SessionUnitId)))
        {
            var sessionId = authorized[result.SessionUnitId].SessionId!.Value;
            var verified = verifiedHeads[sessionId];
            if (verified.Epoch != heads[sessionId].Epoch || scannedFrom[result.SessionUnitId] < verified.MinVersion)
            { result.ResetRequired = true; result.Items.Clear(); result.NextCursor = ProtectCursor(result.SessionUnitId, verified, verified.Version); }
        }
        return results;
    }

    [HttpPost("/api/chat/message/sync-items")]
    [UnitOfWork(isTransactional: false)]
    public async Task<List<MessageSyncItem>> SyncItemsAsync(MessageSyncItemsInput input)
    {
        CheckChangeSync();
        if (input.MessageIds == null || input.MessageIds.Count is < 1 or > 200 || input.MessageIds.Any(x => x <= 0))
            throw new UserFriendlyException("消息校准每批需要 1～200 个有效消息 ID。");
        await GetAndCheckPolicyAsync(GetListPolicyName, input.SessionUnitId);
        return await ChangeRepository.ReadSyncItemsAsync(input.SessionUnitId, input.MessageIds.Distinct().ToList(),
            LazyServiceProvider.LazyGetRequiredService<ICancellationTokenProvider>().Token);
    }
}
