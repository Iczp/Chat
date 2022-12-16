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
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionServices
{
    public class SessionAppService : ChatAppService, ISessionAppService
    {

        protected IRepository<Friendship, Guid> FriendshipRepository { get; }
        protected IRepository<Session, Guid> SessionRepository { get; }

        protected IRepository<Message, Guid> MessageRepository { get; }
        protected ISessionManager SessionManager { get; }

        protected ISessionGenerator SessionGenerator { get; }

        public SessionAppService(
            IRepository<Friendship, Guid> chatObjectRepository,
            ISessionManager sessionManager,
            ISessionGenerator sessionGenerator,
            IRepository<Session, Guid> sessionRepository,
            IRepository<Message, Guid> messageRepository)
        {
            FriendshipRepository = chatObjectRepository;
            SessionManager = sessionManager;
            SessionGenerator = sessionGenerator;
            SessionRepository = sessionRepository;
            MessageRepository = messageRepository;
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

        [HttpPost]
        public async Task<PagedResultDto<SessionDto>> GetSessionsAsync(Guid ownerId, int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            var query = (await SessionRepository.GetQueryableAsync())
                .Where(x => x.MemberList.Any(x => x.OwnerId == ownerId));
            return await GetPagedListAsync<Session, SessionDto>(query, maxResultCount, skipCount, sorting);
        }

        [HttpPost]
        public async Task<PagedResultDto<MessageDto>> GetMessageListAsync(Guid ownerId, int maxResultCount = 10, int skipCount = 0, string sorting = null)
        {
            var query = (await MessageRepository.GetQueryableAsync())
                .Where(x => x.Session.MemberList.Any(m => m.OwnerId == ownerId && m.HistoryFristTime <= x.CreationTime))
                ;

            return await GetPagedListAsync<Message, MessageDto>(query, maxResultCount, skipCount, sorting);
        }


    }
}
