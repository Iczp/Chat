using IczpNet.AbpCommons.DataFilters;
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
using IczpNet.Chat.Specifications;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.SessionServices
{
    public class SessionAppService : ChatAppService, ISessionAppService
    {

        protected IRepository<Friendship, Guid> FriendshipRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }
        protected IRepository<SessionUnit> SessionUnitRepository { get; }

        protected IRepository<Message, Guid> MessageRepository { get; }
        protected ISessionManager SessionManager { get; }


        protected ISessionGenerator SessionGenerator { get; }

        public SessionAppService(
            IRepository<Friendship, Guid> chatObjectRepository,
            ISessionManager sessionManager,
            ISessionGenerator sessionGenerator,
            IRepository<Session, Guid> sessionRepository,
            IRepository<Message, Guid> messageRepository,
            IRepository<SessionUnit> sessionMemberRepository)
        {
            FriendshipRepository = chatObjectRepository;
            SessionManager = sessionManager;
            SessionGenerator = sessionGenerator;
            SessionRepository = sessionRepository;
            MessageRepository = messageRepository;
            SessionUnitRepository = sessionMemberRepository;
        }


        public async Task<PagedResultDto<ChatObjectDto>> GetFriendsAsync(Guid ownerId, bool? isCantacts, int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            var query = (await FriendshipRepository.GetQueryableAsync())
                .Where(x => x.OwnerId == ownerId)
                //.Where(x => x.IsPassive)
                .WhereIf(isCantacts.HasValue, x => x.IsCantacts)
                .Select(x => x.Destination)
                .Distinct()
                ;

            return await GetPagedListAsync<ChatObject, ChatObjectDto>(query, maxResultCount, skipCount, sorting);
        }

        public Task<DateTime> RequestForFriendshipAsync(Guid ownerId, Guid friendId, string message)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<OpenedRecorderDto> SetOpenedAsync(OpenedRecorderInput input)
        {
            var entity = await SessionManager.SetOpenedAsync(input.OwnerId, input.DestinationId, input.MessageId, input.DeviceId);

            return ObjectMapper.Map<OpenedRecorder, OpenedRecorderDto>(entity);
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionDto>> GetSessionsAsync(SessionGetListInput input)
        {
            var query = (await SessionRepository.GetQueryableAsync())
                .Where(x => x.UnitList.Any(m => m.OwnerId == input.OwnerId))
                ;
            return await GetPagedListAsync<Session, SessionDto>(query, input);
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionUnitDto>> GetSessionUnitsAsync(SessionGetListInput input)
        {
            var query = (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => x.OwnerId == input.OwnerId)
                ;
            return await GetPagedListAsync<SessionUnit, SessionUnitDto>(query, input);
        }

        [HttpGet]
        public async Task<PagedResultDto<MessageDto>> GetMessageListAsync(SessionMessageGetListInput input)
        {
            var query = (await MessageRepository.GetQueryableAsync())
                .Where(new OwnerMessageSpecification(input.OwnerId).ToExpression())
                .WhereIf(input.SessionId.HasValue, new SessionMessageSpecification(input.SessionId.GetValueOrDefault()).ToExpression())
                .WhereIf(input.IsUnreaded, new UnreadedMessageSpecification(input.OwnerId).ToExpression())
                .WhereIf(input.SenderId.HasValue, new SenderMessageSpecification(input.SenderId.GetValueOrDefault()).ToExpression())
                ;

            return await GetPagedListAsync<Message, MessageDto>(query, input);
        }


        [HttpPost]
        public async Task<List<SessionDto>> CreateSessionAsync()
        {
            var entitys = await SessionGenerator.CreateSessionAsync();
            return ObjectMapper.Map<List<Session>, List<SessionDto>>(entitys);
        }
    }
}
