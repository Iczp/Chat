using IczpNet.AbpCommons;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionSections.SessionRequests
{
    public class SessionRequestManager : DomainService, ISessionRequestManager
    {
        protected IRepository<SessionRequest, Guid> Repository { get; }

        public SessionRequestManager(IRepository<SessionRequest, Guid> repository)
        {
            Repository = repository;
        }

        public async Task<SessionRequest> HandleRequestAsync(Guid sessionRequestId, bool isAgreed, string handlMessage, Guid? handlerSessionUnitId)
        {
            var sessionRequest = await Repository.GetAsync(sessionRequestId);

            Assert.If(sessionRequest.IsHandled, $"Already been handled:IsAgreed={sessionRequest.IsAgreed}");

            if (isAgreed)
            {
                
                //handle...  
                // addSessionUnit

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
