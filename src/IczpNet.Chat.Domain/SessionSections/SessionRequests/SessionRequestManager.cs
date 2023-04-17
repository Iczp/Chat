using IczpNet.AbpCommons;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionRequests
{
    public class SessionRequestManager : DomainService, ISessionRequestManager
    {
        protected IRepository<SessionRequest, Guid> Repository { get; }
        protected ISessionUnitRepository SessionUnitRepository { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        protected ISessionManager SessionManager { get; }
        public SessionRequestManager(IRepository<SessionRequest, Guid> repository, 
            ISessionUnitRepository sessionUnitRepository, 
            ISessionUnitManager sessionUnitManager, 
            ISessionManager sessionManager)
        {
            Repository = repository;
            SessionUnitRepository = sessionUnitRepository;
            SessionUnitManager = sessionUnitManager;
            SessionManager = sessionManager;
        }

        public async Task<SessionRequest> HandleRequestAsync(Guid sessionRequestId, bool isAgreed, string handlMessage, Guid? handlerSessionUnitId)
        {
            var sessionRequest = await Repository.GetAsync(sessionRequestId);

            Assert.If(sessionRequest.IsHandled, $"Already been handled:IsAgreed={sessionRequest.IsAgreed}");

            if (isAgreed)
            {

                //handle...  
                // addSessionUnit
                var sessionUnit = await SessionUnitManager.FindAsync(sessionRequest.OwnerId, sessionRequest.DestinationId.Value);
                if (sessionUnit != null)
                {

                }

                sessionRequest.AgreeRequest(handlMessage, handlerSessionUnitId);
            }
            else
            {
                sessionRequest.DisagreeRequest(handlMessage, handlerSessionUnitId);
            }

            //await FriendshipRequestRepository.UpdateAsync(sessionRequest, autoSave: true);

            //await DeleteFriendshipRequestAsync(sessionRequest.OwnerId, sessionRequest.DestinationId.Value);

            return sessionRequest;
        }
    }
}
