using System.Threading.Tasks;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.CallCenters
{
    public interface ICallCenterAppService : IApplicationService
    {

        Task TransferToAsync(Guid sessionId, long destinationId);
    }
}
