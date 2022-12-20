using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices
{

    public class SessionUnitAppService : ChatAppService, ISessionUnitAppService
    {
        protected IRepository<Friendship, Guid> FriendshipRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        protected IRepository<SessionUnit, Guid> Repository { get; }
        protected IRepository<Message, Guid> MessageRepository { get; }
        protected ISessionManager SessionManager { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        protected ISessionGenerator SessionGenerator { get; }
        public virtual string GetListPolicyName { get; private set; }
        public virtual string GetPolicyName { get; private set; }
        public virtual string GetDetailPolicyName { get; private set; }
        public virtual string SetReadedPolicyName { get; private set; }
        public virtual string RemoveSessionPolicyName { get; private set; }
        public virtual string ClearMessagePolicyName { get; private set; }
        public virtual string DeleteMessagePolicyName { get; private set; }

        public SessionUnitAppService(
            IRepository<Friendship, Guid> chatObjectRepository,
            ISessionManager sessionManager,
            ISessionGenerator sessionGenerator,
            IRepository<Session, Guid> sessionRepository,
            IRepository<Message, Guid> messageRepository,
            IRepository<SessionUnit, Guid> repository,
            ISessionUnitManager sessionUnitManager)
        {
            FriendshipRepository = chatObjectRepository;
            SessionManager = sessionManager;
            SessionGenerator = sessionGenerator;
            SessionRepository = sessionRepository;
            MessageRepository = messageRepository;
            Repository = repository;
            SessionUnitManager = sessionUnitManager;
        }

        protected override Task CheckPolicyAsync(string policyName)
        {
            return base.CheckPolicyAsync(policyName);
        }

        [HttpGet]
        public virtual async Task<PagedResultDto<SessionUnitDto>> GetListAsync(SessionUnitGetListInput input)
        {
            await CheckPolicyAsync(GetListPolicyName);
            var query = (await Repository.GetQueryableAsync())


                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                ;
            //...
            query = query.OrderBy(x => x.Id);

            return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input);
        }

        [HttpGet]
        public virtual async Task<SessionUnitDto> GetAsync(Guid id)
        {
            await CheckPolicyAsync(GetPolicyName);
            var entity = await Repository.GetAsync(id);
            return await MapToDtoAsync(entity);
        }

        [HttpGet]
        public virtual async Task<SessionUnitDetailDto> GetDetailAsync(Guid id)
        {
            await CheckPolicyAsync(GetDetailPolicyName);
            var entity = await Repository.GetAsync(id);
            return ObjectMapper.Map<SessionUnit, SessionUnitDetailDto>(entity);
        }

        private Task<SessionUnitDto> MapToDtoAsync(SessionUnit entity)
        {
            return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitDto>(entity));
        }

        [HttpPost]
        public virtual async Task<SessionUnitDto> SetReadedAsync(Guid id, Guid messageId, bool isForce = false)
        {
            await CheckPolicyAsync(SetReadedPolicyName);
            var entity = await Repository.GetAsync(id);
            await SessionUnitManager.SetReadedAsync(entity, messageId, isForce);
            return await MapToDtoAsync(entity);
        }

        [HttpPost]
        public virtual async Task<SessionUnitDto> RemoveSessionAsync(Guid id)
        {
            await CheckPolicyAsync(RemoveSessionPolicyName);
            var entity = await Repository.GetAsync(id);
            await SessionUnitManager.RemoveSessionAsync(entity);
            return await MapToDtoAsync(entity);
        }

        [HttpPost]
        public virtual async Task<SessionUnitDto> KillSessionAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);
            await SessionUnitManager.KillSessionAsync(entity);
            return await MapToDtoAsync(entity);
        }

        [HttpPost]
        public virtual async Task<SessionUnitDto> ClearMessageAsync(Guid id)
        {
            await CheckPolicyAsync(ClearMessagePolicyName);
            var entity = await Repository.GetAsync(id);
            await SessionUnitManager.ClearMessageAsync(entity);
            return await MapToDtoAsync(entity);
        }

        [HttpPost]
        public virtual async Task<SessionUnitDto> DeleteMessageAsync(Guid id, Guid messageId)
        {
            await CheckPolicyAsync(DeleteMessagePolicyName);
            throw new NotImplementedException();
        }
    }
}
