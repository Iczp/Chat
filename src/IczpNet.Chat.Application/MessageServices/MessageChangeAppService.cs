using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Threading;
using Volo.Abp.Uow;
using Volo.Abp.Users;

namespace IczpNet.Chat.MessageServices;

/// <summary>
/// 消息变更同步服务
/// </summary>
[Authorize]
public class MessageChangeAppService : ChatAppService, IMessageChangeAppService
{
    protected IMessageChangeRepository ChangeRepository =>
        LazyServiceProvider.LazyGetRequiredService<IMessageChangeRepository>();

    protected CancellationToken ChangeCancellationToken =>
        LazyServiceProvider.LazyGetRequiredService<ICancellationTokenProvider>().Token;

    protected MessageOptions GetChangeSyncOptions()
    {
        var options = LazyServiceProvider.LazyGetRequiredService<IOptions<MessageOptions>>().Value;
        if (!options.ChangeSyncEnabled)
            throw new EntityNotFoundException("Message Change Sync is disabled.");
        return options;
    }

    private static string ToSeq(long version) => version.ToString(CultureInfo.InvariantCulture);

    private static string GetEpoch(MessageOptions options)
    {
        var value = $"Chat.MessageChanges:{options.ChangeSyncV2Generation}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }

    [HttpPost]
    [UnitOfWork(isTransactional: false)]
    public virtual async Task<MessageChangePullDto> PullAsync(MessageChangePullInput input)
    {
        var options = GetChangeSyncOptions();
        if (input.MaxResultCount is < 1 or > 200 ||
            !long.TryParse(input.AfterMessageChangeId, NumberStyles.None, CultureInfo.InvariantCulture, out var after) || after < 0)
            throw new UserFriendlyException("消息同步游标或批次大小无效。");

        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));
        var token = ChangeCancellationToken;
        var headChangeId = await ChangeRepository.GetHeadChangeIdAsync(token);
        var epoch = GetEpoch(options);
        if (string.IsNullOrWhiteSpace(input.Epoch) ||
            !string.Equals(input.Epoch, epoch, StringComparison.OrdinalIgnoreCase) ||
            after > headChangeId)
        {
            return new MessageChangePullDto(
                epoch,
                ToSeq(headChangeId),
                input.AfterMessageChangeId,
                HasMore: false,
                ResetRequired: true,
                Items: []);
        }

        var page = await ChangeRepository.PullChangesAsync(
            input.ChatObjectId,
            after,
            headChangeId,
            input.MaxResultCount,
            token);

        var verifiedHead = await ChangeRepository.GetHeadChangeIdAsync(token);
        if (verifiedHead < headChangeId)
        {
            return new MessageChangePullDto(
                epoch,
                ToSeq(verifiedHead),
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
            },
            row.Description)).ToList();

        return new MessageChangePullDto(
            epoch,
            ToSeq(headChangeId),
            ToSeq(page.NextCursor),
            page.HasMore,
            ResetRequired: false,
            Items: items);
    }

    [HttpPost]
    [UnitOfWork(isTransactional: true)]
    public virtual async Task<MessageChangeAckDto> AckAsync(MessageChangeAckInput input)
    {
        var options = GetChangeSyncOptions();
        if (!long.TryParse(input.LastAppliedMessageChangeId, NumberStyles.None, CultureInfo.InvariantCulture, out var ackId) || ackId < 0)
            throw new UserFriendlyException("消息同步确认游标无效。");

        var userId = CurrentUser.GetId();
        await CheckPolicyForUserAsync(input.ChatObjectId, () => CheckPolicyAsync(GetListPolicyName));
        var token = ChangeCancellationToken;
        var headChangeId = await ChangeRepository.GetHeadChangeIdAsync(token);
        var epoch = GetEpoch(options);
        if (!string.Equals(input.Epoch, epoch, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await ChangeRepository.GetCursorAsync(CurrentTenant.Id, userId, input.ChatObjectId, input.DeviceId.Trim(), token);
            return new MessageChangeAckDto(
                epoch,
                ToSeq(headChangeId),
                ToSeq(existing?.LastAppliedMessageChangeId ?? 0),
                existing?.IsInitialized ?? false,
                ResetRequired: true);
        }

        if (ackId > headChangeId)
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
            ToSeq(headChangeId),
            ToSeq(cursor.LastAppliedMessageChangeId),
            cursor.IsInitialized,
            ResetRequired: false);
    }
}
