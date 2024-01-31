using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Services;
using IczpNet.Chat.SessionUnits.Dtos;

namespace IczpNet.Chat.CallCenters
{
    public interface ICallCenterAppService : IApplicationService
    {

        /// <summary>
        /// 转接
        /// </summary>
        /// <param name="sessionUnitId">当前会话单元Id</param>
        /// <param name="destinationId">目标会话单元Id</param>
        /// <returns></returns>
        Task<SessionUnitOwnerDetailDto> TransferToAsync(Guid sessionUnitId, long destinationId);
    }
}
