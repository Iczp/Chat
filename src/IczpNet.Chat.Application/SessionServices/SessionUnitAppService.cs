using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.SessionSections;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using IczpNet.Chat.SessionSections.OpenedRecordes.Dtos;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using IczpNet.Chat.Specifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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


        protected ISessionGenerator SessionGenerator { get; }

        public SessionUnitAppService(
            IRepository<Friendship, Guid> chatObjectRepository,
            ISessionManager sessionManager,
            ISessionGenerator sessionGenerator,
            IRepository<Session, Guid> sessionRepository,
            IRepository<Message, Guid> messageRepository,
            IRepository<SessionUnit, Guid> repository)
        {
            FriendshipRepository = chatObjectRepository;
            SessionManager = sessionManager;
            SessionGenerator = sessionGenerator;
            SessionRepository = sessionRepository;
            MessageRepository = messageRepository;
            Repository = repository;
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
            return ObjectMapper.Map<SessionUnit, SessionUnitDto>(entity);
        }
    }
}
