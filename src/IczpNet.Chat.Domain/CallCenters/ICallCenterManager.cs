using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.CallCenters
{
    public interface ICallCenterManager : IDomainService
    {

        /// <summary>
        /// 转接
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="destinationId"></param>
        /// <returns></returns>
        Task TransferToAsync(Guid sessionId, long destinationId);
    }
}
