using IczpNet.Chat.SessionUnits;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.CallCenters;

public interface ICallCenterManager : IDomainService
{

    /// <summary>
    /// 转接
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="waiterId"></param>
    /// <param name="isNotice"></param>
    /// <returns></returns>
    Task<SessionUnit> TransferToAsync(Guid sessionUnitId, long waiterId, bool isNotice = true);
}
