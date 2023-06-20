using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Services;

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
        Task TransferToAsync(Guid sessionUnitId, long destinationId);
    }
}
