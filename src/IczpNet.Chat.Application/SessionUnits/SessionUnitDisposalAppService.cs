using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnitSettings;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话处置增量拉取与生命周期服务
/// </summary>
[Authorize]
public class SessionUnitDisposalAppService(
    IRepository<SessionUnitDisposal, long> disposalRepository) : ChatAppService, ISessionUnitDisposalAppService
{
    protected IRepository<SessionUnitDisposal, long> DisposalRepository { get; } = disposalRepository;

    /// <summary>
    /// 增量拉取会话处置记录（Remove/Delete/Hide/ClearMessage）
    /// </summary>
    [HttpPost]
    public virtual async Task<SessionUnitDisposalPagedResultDto> GetListAsync(SessionUnitDisposalGetListInput input)
    {
        await CheckPolicyForUserAsync(input.OwnerId, () => CheckPolicyAsync(GetListPolicyName));

        var queryable = await DisposalRepository.GetQueryableAsync();

        var query = queryable
            .Where(x => x.OwnerId == input.OwnerId && x.Id > input.AfterId);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var headId = await AsyncExecuter.FirstOrDefaultAsync(
            queryable
                .Where(x => x.OwnerId == input.OwnerId)
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id));

        var maxResultCount = input.MaxResultCount > 0 ? Math.Min(input.MaxResultCount, 200) : 50;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.Id)
                 .Take(maxResultCount)
                 .Select(x => new SessionUnitDisposalDto
                 {
                     Id = x.Id,
                     OwnerId = x.OwnerId,
                     SessionUnitId = x.SessionUnitId,
                     SessionId = x.SessionId,
                     DestinationId = x.DestinationId,
                     DisposalType = x.DisposalType,
                     Reason = x.Reason,
                     OperatorId = x.OperatorId,
                     ClearBeforeTime = x.ClearBeforeTime,
                     ClearMaxMessageId = x.ClearMaxMessageId,
                     Description = x.Description,
                     CreationTime = x.CreationTime
                 }));

        return new SessionUnitDisposalPagedResultDto(totalCount, items, headId);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitDisposalDto> RemoveAsync(Guid sessionUnitId, string reason = null)
    {
        var unit = await SessionUnitManager.GetAsync(sessionUnitId);
        if (CurrentUser.Id.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            if (!ownedIds.Contains(unit.OwnerId))
            {
                throw new Volo.Abp.Authorization.AbpAuthorizationException("无权操作该会话单元");
            }
        }

        if (unit.Setting?.IsCreator == true && unit.OwnerId != unit.DestinationId && unit.SessionId.HasValue)
        {
            var unitRepoCheck = LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();
            var hasOtherRealMember = await AsyncExecuter.AnyAsync(
                (await unitRepoCheck.GetQueryableAsync()).Where(x =>
                    x.SessionId == unit.SessionId.Value
                    && x.Id != unit.Id
                    && x.OwnerId != x.DestinationId
                    && x.Setting != null
                    && x.Setting.IsEnabled
                    && !x.Setting.IsKilled));
            if (hasOtherRealMember)
            {
                throw new Volo.Abp.UserFriendlyException("群主退群前请先转让群主，或先解散群聊。");
            }
        }

        var settingManager = LazyServiceProvider.LazyGetRequiredService<ISessionUnitSettingManager>();
        await settingManager.KillAsync(sessionUnitId, KillTypes.OwnerQuit);

        var queryable = await DisposalRepository.GetQueryableAsync();
        var latest = await AsyncExecuter.FirstOrDefaultAsync(
            queryable
                .Where(x => x.SessionUnitId == sessionUnitId)
                .OrderByDescending(x => x.Id));

        if (latest == null)
        {
            return new SessionUnitDisposalDto
            {
                SessionUnitId = sessionUnitId,
                OwnerId = unit.OwnerId,
                SessionId = unit.SessionId,
                DestinationId = unit.DestinationId,
                DisposalType = SessionUnitDisposalType.Remove,
                Reason = SessionUnitDisposalReason.MemberQuit,
                Description = reason ?? "退出群聊",
                CreationTime = DateTime.UtcNow
            };
        }

        return MapToDto(latest);
    }

    /// <inheritdoc/>
    [HttpPost]
    public virtual async Task<SessionUnitDisposalDto> QuitAndDeleteAsync(Guid sessionUnitId)
    {
        var unit = await SessionUnitManager.GetAsync(sessionUnitId);
        if (CurrentUser.Id.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            if (!ownedIds.Contains(unit.OwnerId))
            {
                throw new Volo.Abp.Authorization.AbpAuthorizationException("无权操作该会话单元");
            }
        }

        if (unit.Setting?.IsCreator == true && unit.OwnerId != unit.DestinationId && unit.SessionId.HasValue)
        {
            var unitRepoCheck = LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();
            var hasOtherRealMember = await AsyncExecuter.AnyAsync(
                (await unitRepoCheck.GetQueryableAsync()).Where(x =>
                    x.SessionId == unit.SessionId.Value
                    && x.Id != unit.Id
                    && x.OwnerId != x.DestinationId
                    && x.Setting != null
                    && x.Setting.IsEnabled
                    && !x.Setting.IsKilled));
            if (hasOtherRealMember)
            {
                throw new Volo.Abp.UserFriendlyException("群主退群前请先转让群主，或先解散群聊。");
            }
        }

        var settingManager = LazyServiceProvider.LazyGetRequiredService<ISessionUnitSettingManager>();
        await settingManager.KillAsync(sessionUnitId, KillTypes.OwnerQuit);

        var unitRepo = LazyServiceProvider.LazyGetRequiredService<ISessionUnitRepository>();
        await unitRepo.DeleteAsync(unit, autoSave: true);

        var disposalManager = LazyServiceProvider.LazyGetRequiredService<ISessionUnitDisposalManager>();
        var disposal = await disposalManager.RecordDisposalAsync(
            ownerId: unit.OwnerId,
            sessionUnitId: unit.Id,
            sessionId: unit.SessionId,
            destinationId: unit.DestinationId,
            disposalType: SessionUnitDisposalType.Delete,
            reason: SessionUnitDisposalReason.UserDeletedSession,
            description: "退出并删除会话");

        return MapToDto(disposal);
    }

    /// <inheritdoc/>
    [HttpGet]
    public virtual async Task<SessionUnitDisposalDto> GetStatusAsync(Guid sessionUnitId)
    {
        var unit = await SessionUnitManager.GetAsync(sessionUnitId);
        if (CurrentUser.Id.HasValue)
        {
            var ownedIds = await ChatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
            if (!ownedIds.Contains(unit.OwnerId))
            {
                throw new Volo.Abp.Authorization.AbpAuthorizationException("无权查看该会话单元处置状态");
            }
        }

        var queryable = await DisposalRepository.GetQueryableAsync();
        var latest = await AsyncExecuter.FirstOrDefaultAsync(
            queryable
                .Where(x => x.SessionUnitId == sessionUnitId)
                .OrderByDescending(x => x.Id));

        if (latest != null)
        {
            return MapToDto(latest);
        }

        return new SessionUnitDisposalDto
        {
            SessionUnitId = sessionUnitId,
            OwnerId = unit.OwnerId,
            SessionId = unit.SessionId,
            DestinationId = unit.DestinationId,
            DisposalType = unit.Setting?.IsKilled == true ? SessionUnitDisposalType.Remove : SessionUnitDisposalType.Normal,
            Reason = unit.Setting?.IsKilled == true ? SessionUnitDisposalReason.MemberQuit : SessionUnitDisposalReason.MemberRejoined,
            CreationTime = unit.Setting?.KillTime ?? unit.CreationTime
        };
    }

    protected virtual SessionUnitDisposalDto MapToDto(SessionUnitDisposal entity)
    {
        if (entity == null) return null;
        return new SessionUnitDisposalDto
        {
            Id = entity.Id,
            OwnerId = entity.OwnerId,
            SessionUnitId = entity.SessionUnitId,
            SessionId = entity.SessionId,
            DestinationId = entity.DestinationId,
            DisposalType = entity.DisposalType,
            Reason = entity.Reason,
            OperatorId = entity.OperatorId,
            ClearBeforeTime = entity.ClearBeforeTime,
            ClearMaxMessageId = entity.ClearMaxMessageId,
            Description = entity.Description,
            CreationTime = entity.CreationTime
        };
    }
}
