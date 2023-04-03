using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.CallCenters
{
    public class CallCenterManager : DomainService, ICallCenterManager
    {

        /// <inheritdoc/>
        public Task TransferToAsync(Guid sessionId, long destinationId)
        {
            throw new NotImplementedException();
        }
    }
}
