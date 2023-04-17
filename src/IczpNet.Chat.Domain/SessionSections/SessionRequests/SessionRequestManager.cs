using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using System.Linq;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionSections.SessionRequests
{
    public class SessionRequestManager : DomainService, ISessionRequestManager
    {
        protected IRepository<SessionRequest, Guid> Repository { get; }
        protected ISessionUnitRepository SessionUnitRepository { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        protected ISessionManager SessionManager { get; }
        protected IMessageSender MessageSender { get; }
        protected IChatObjectManager ChatObjectManager { get; }
        protected ISessionGenerator SessionGenerator { get; }
        protected IRepository<SessionPermissionRoleGrant> SessionPermissionRoleGrantRepository { get; }
        protected IRepository<SessionPermissionUnitGrant> SessionPermissionUnitGrantRepository { get; }

        protected List<ChatObjectTypeEnums> DisallowCreateList { get; set; } = new List<ChatObjectTypeEnums>() {
            ChatObjectTypeEnums.Robot,
            ChatObjectTypeEnums.Anonymous,
            ChatObjectTypeEnums.Room,
            ChatObjectTypeEnums.Square,
        };

        public SessionRequestManager(IRepository<SessionRequest, Guid> repository,
            ISessionUnitRepository sessionUnitRepository,
            ISessionUnitManager sessionUnitManager,
            ISessionManager sessionManager,
            IMessageSender messageSender,
            IChatObjectManager chatObjectManager,
            ISessionGenerator sessionGenerator,
            IRepository<SessionPermissionRoleGrant> sessionPermissionRoleGrantRepository,
            IRepository<SessionPermissionUnitGrant> sessionPermissionUnitGrantRepository)
        {
            Repository = repository;
            SessionUnitRepository = sessionUnitRepository;
            SessionUnitManager = sessionUnitManager;
            SessionManager = sessionManager;
            MessageSender = messageSender;
            ChatObjectManager = chatObjectManager;
            SessionGenerator = sessionGenerator;
            SessionPermissionRoleGrantRepository = sessionPermissionRoleGrantRepository;
            SessionPermissionUnitGrantRepository = sessionPermissionUnitGrantRepository;
        }

        public virtual async Task<SessionRequest> CreateRequestAsync(long ownerId, long destinationId, string requestMessage)
        {
            Assert.If(await SessionUnitManager.FindAsync(ownerId, destinationId) != null, "Already a friend");

            var owner = await ChatObjectManager.GetAsync(ownerId);

            Assert.If(DisallowCreateList.Contains(owner.ObjectType.Value), $"The owner's ObjectType '{owner.ObjectType}' disallow create session request. OwnerId:{ownerId}");

            var destination = await ChatObjectManager.GetAsync(destinationId);

            var entity = await Repository.InsertAsync(new SessionRequest(owner, destination, requestMessage), autoSave: true);

            switch (destination.ObjectType)
            {
                case ChatObjectTypeEnums.Personal:
                case ChatObjectTypeEnums.Customer:
                case ChatObjectTypeEnums.ShopKeeper:
                    await SendForPersonalAsync(owner, destination, entity);
                    break;
                case ChatObjectTypeEnums.Room:
                case ChatObjectTypeEnums.Square:
                    await SendForRoomOrSquareAsync(owner, destination, entity);
                    break;
                default:
                    Assert.If(true, $"The owner's ObjectType '{owner.ObjectType}' disallow create session request. OwnerId:{ownerId}");
                    break;
            }

            return entity;
        }

        protected virtual async Task SendForRoomOrSquareAsync(ChatObject owner, ChatObject destination, SessionRequest sessionRequest)
        {
            var assistant = await ChatObjectManager.GetOrAddPrivateAssistantAsync();

            // room creator and room manager

            var roleIdList = (await SessionPermissionRoleGrantRepository.GetQueryableAsync())
                  .Where(x => x.DefinitionId == SessionPermissionDefinitionConsts.SessionRequest.Handle && x.IsEnabled)
                  .Select(x => x.RoleId);

            var sessionUnitIdList = (await SessionPermissionUnitGrantRepository.GetQueryableAsync())
                   .Where(x => x.DefinitionId == SessionPermissionDefinitionConsts.SessionRequest.Handle && x.IsEnabled)
                   .Select(x => x.SessionUnitId);

            var managerAndCretorList = (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => x.DestinationId == destination.Id)
                .Where(x => x.SessionUnitRoleList.Any(d => roleIdList.Contains(d.SessionRoleId)) || sessionUnitIdList.Contains(x.Id) || x.IsCreator)
                .Select(x => x.Owner)
                .Distinct()
                .ToList();

            if (!managerAndCretorList.Any())
            {
                Logger.LogWarning("No manager or creator found,");
            }

            foreach (var managerOrCretor in managerAndCretorList)
            {
                await SessionGenerator.MakeAsync(assistant, managerOrCretor);

                var senderSessionUnit = await SessionUnitManager.FindAsync(assistant.Id, managerOrCretor.Id);

                await MessageSender.SendLinkAsync(senderSessionUnit, new MessageSendInput<LinkContentInfo>()
                {
                    Content = new LinkContentInfo()
                    {
                        Title = $"{owner?.Name} 请求加入群聊'{destination.Name}'",
                        Url = $"app://sesson-request/detail?id={sessionRequest.Id}"
                    }
                });
            }
        }

        protected virtual async Task SendForPersonalAsync(ChatObject owner, IChatObject destination, SessionRequest sessionRequest)
        {
            var assistant = await ChatObjectManager.GetOrAddPrivateAssistantAsync();

            await SessionGenerator.MakeAsync(assistant, destination);

            var sessionUnit = await SessionUnitManager.FindAsync(assistant.Id, destination.Id);

            await MessageSender.SendLinkAsync(sessionUnit, new MessageSendInput<LinkContentInfo>()
            {
                Content = new LinkContentInfo()
                {
                    Title = $"{owner?.Name} 请求加为好友",
                    Url = $"app://sesson-request/detail?id={sessionRequest.Id}"
                }
            });
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
