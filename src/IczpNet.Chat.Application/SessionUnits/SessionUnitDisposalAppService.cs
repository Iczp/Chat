using System;
using System.Linq;
using System.Threading.Tasks;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话处置增量拉取服务
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
                     Description = x.Description,
                     CreationTime = x.CreationTime
                 }));

        return new SessionUnitDisposalPagedResultDto(totalCount, items, headId);
    }
}
