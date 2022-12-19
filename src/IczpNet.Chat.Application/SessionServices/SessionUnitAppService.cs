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

        [HttpGet]
        public async Task<PagedResultDto<SessionUnitDto>> GetListAsync(SessionUnitGetListInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                ;
            return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input);
        }

        [HttpGet]
        public async Task<SessionUnitDto> GetListAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            return await MapToEntityAsync(entity);
        }


        private Task<SessionUnitDto> MapToEntityAsync(SessionUnit entity)
        {
            return Task.FromResult(ObjectMapper.Map<SessionUnit, SessionUnitDto>(entity));
        }

        [HttpPost]
        public async Task<SessionUnitDto> SetReadedAsync(Guid id, Guid messageId)
        {
            var entity = await Repository.GetAsync(id);

            await SessionUnitManager.SetReadedAsync(entity, messageId);

            return await MapToEntityAsync(entity);
        }

        [HttpPost]
        public async Task<SessionUnitDto> RemoveAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            await SessionUnitManager.RemoveAsync(entity);

            return await MapToEntityAsync(entity);
        }

        [HttpPost]
        public async Task<SessionUnitDto> KillAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            await SessionUnitManager.KillAsync(entity);

            return await MapToEntityAsync(entity);
        }

        [HttpPost]
        public async Task<SessionUnitDto> ClearAsync(Guid id)
        {
            var entity = await Repository.GetAsync(id);

            await SessionUnitManager.KillAsync(entity);

            return await MapToEntityAsync(entity);
        }

        [HttpPost]
        public Task<SessionUnitDto> DeleteMessageAsync(Guid id, Guid messageId)
        {
            throw new NotImplementedException();
        }
    }
}
