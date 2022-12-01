using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.Friends;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Friendships.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class FriendshipAppService
        : CrudChatAppService<
            Friendship,
            FriendshipDetailDto,
            FriendshipDto,
            Guid,
            FriendshipGetListInput,
            FriendshipCreateInput,
            FriendshipUpdateInput>,
        IFriendshipAppService
    {
        public FriendshipAppService(IRepository<Friendship, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<Friendship>> CreateFilteredQueryAsync(FriendshipGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))

                ;
        }
    }
}
