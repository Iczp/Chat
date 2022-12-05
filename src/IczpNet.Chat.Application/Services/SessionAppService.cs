using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.SessionSections;
using IczpNet.Chat.SessionSections.Friendships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class SessionAppService : ChatAppService, ISessionAppService
    {

        protected IRepository<Friendship, Guid> FriendshipRepository { get; }
        protected ISessionManager SessionManager { get; }

        public SessionAppService(
            IRepository<Friendship, Guid> chatObjectRepository,
            ISessionManager sessionManager)
        {
            FriendshipRepository = chatObjectRepository;
            SessionManager = sessionManager;
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

            var totalCount = await AsyncExecuter.CountAsync(query);

            if (!sorting.IsNullOrWhiteSpace())
            {
                query = query.OrderBy(sorting);
            }
            query = query.PageBy(skipCount, maxResultCount);

            var entities = await AsyncExecuter.ToListAsync(query);

            var items = ObjectMapper.Map<List<ChatObject>, List<ChatObjectDto>>(entities);

            return new PagedResultDto<ChatObjectDto>(totalCount, items);
        }

        public Task<DateTime> RequestForFriendshipAsync(Guid ownerId, Guid friendId, string message)
        {
            throw new NotImplementedException();
        }
    }
}
