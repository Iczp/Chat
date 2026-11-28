using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionRequests;

public class SessionRequestAdminManager(
    IRepository<SessionRequest, Guid> repository,
    ISessionRequestManager sessionRequestManager,
    ISessionUnitRepository sessionUnitRepository)
    : DomainService, ISessionRequestAdminManager, ITransientDependency
{
    protected IRepository<SessionRequest, Guid> Repository { get; } = repository;
    protected ISessionRequestManager SessionRequestManager { get; } = sessionRequestManager;
    protected ISessionUnitRepository SessionUnitRepository { get; } = sessionUnitRepository;

    public virtual async Task<SessionRequestAdminListResult> GetAdminListAsync(
        string keyword,
        long? ownerId,
        long? destinationId,
        bool? isHandled,
        bool? isAgreed,
        bool? isExpired,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Owner)
            .Include(x => x.Destination)
            .WhereIf(ownerId.HasValue, x => x.OwnerId == ownerId.Value)
            .WhereIf(destinationId.HasValue, x => x.DestinationId == destinationId.Value)
            .WhereIf(isHandled.HasValue, x => x.IsHandled == isHandled.Value)
            .WhereIf(isAgreed.HasValue, x => x.IsAgreed == isAgreed.Value)
            .WhereIf(isExpired.HasValue, x => x.IsExpired == isExpired.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(x =>
                (x.RequestMessage != null && x.RequestMessage.Contains(kw)) ||
                (x.HandleMessage != null && x.HandleMessage.Contains(kw)) ||
                (x.Owner != null && x.Owner.Name.Contains(kw)) ||
                (x.Destination != null && x.Destination.Name.Contains(kw)));
        }

        var totalCount = await AsyncExecuter.LongCountAsync(query, cancellationToken);
        if (totalCount == 0)
        {
            return new SessionRequestAdminListResult();
        }

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(x => x.CreationTime)
                .Skip(Math.Max(0, skipCount))
                .Take(Math.Clamp(maxResultCount > 0 ? maxResultCount : 20, 1, 200)),
            cancellationToken);

        return new SessionRequestAdminListResult
        {
            TotalCount = totalCount,
            Items = items
        };
    }

    public virtual async Task<SessionRequest> GetAdminDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = (await Repository.GetQueryableAsync())
            .Include(x => x.Owner)
            .Include(x => x.Destination)
            .Where(x => x.Id == id);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query, cancellationToken);
        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(SessionRequest), id);
        }

        return entity;
    }

    public virtual async Task<SessionRequest> HandleAdminAsync(
        Guid id,
        bool isAgreed,
        string handleMessage,
        SessionRequestAdminScope scope = null,
        CancellationToken cancellationToken = default)
    {
        if (scope != null && scope.TenantId != CurrentTenant.Id)
        {
            throw new AbpAuthorizationException("跨租户审批被拒绝。");
        }

        var entity = await GetAdminDetailAsync(id, cancellationToken);
        if (entity.IsHandled)
        {
            throw new BusinessException("ChatAdmin:SessionRequestAlreadyHandled", "该申请已处理，不允许重复审批。");
        }
        if (entity.IsExpired)
        {
            throw new BusinessException("ChatAdmin:SessionRequestExpired", "该申请已过期，无法审批。");
        }

        Guid? handlerSessionUnitId = null;
        if (scope != null && !scope.HasManageAll)
        {
            if (scope.AllowedOwnerIds.Count == 0)
            {
                throw new AbpAuthorizationException("无权审批该申请。");
            }

            if (entity.Destination?.ObjectType is ChatObjectTypeEnums.Room or ChatObjectTypeEnums.Square)
            {
                var handlerUnit = await AsyncExecuter.FirstOrDefaultAsync(
                    (await SessionUnitRepository.GetQueryableAsync())
                        .Where(x => scope.AllowedOwnerIds.Contains(x.OwnerId)
                            && x.DestinationId == entity.DestinationId
                            && x.Setting.IsEnabled
                            && !x.Setting.IsKilled),
                    cancellationToken);
                if (handlerUnit == null)
                {
                    throw new AbpAuthorizationException("无权审批该群聊申请。");
                }
                handlerSessionUnitId = handlerUnit.Id;
            }
            else if (!entity.DestinationId.HasValue || !scope.AllowedOwnerIds.Contains(entity.DestinationId.Value))
            {
                throw new AbpAuthorizationException("无权审批该好友申请。");
            }
        }

        await SessionRequestManager.HandleRequestAsync(entity.Id, isAgreed, handleMessage, handlerSessionUnitId);
        return await GetAdminDetailAsync(id, cancellationToken);
    }
}
