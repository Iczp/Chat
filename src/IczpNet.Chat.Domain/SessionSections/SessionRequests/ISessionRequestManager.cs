using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionRequests
{
    public interface ISessionRequestManager
    {
        Task<SessionRequest> CreateRequestAsync(long ownerId, long destinationId, string requestMessage);

        Task<SessionRequest> HandleRequestAsync(Guid sessionRequestId, bool isAgreed, string handlMessage, Guid? handlerSessionUnitId);
    }
}
