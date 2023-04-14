using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionRequests
{
    public interface ISessionRequestManager
    {

        Task<SessionRequest> HandleRequestAsync(Guid sessionRequestId, bool isAgreed, string handlMessage, Guid? handlerSessionUnitId);
    }
}
