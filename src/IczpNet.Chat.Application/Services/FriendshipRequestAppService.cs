using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.Friends;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class FriendshipRequestAppService
        : CrudChatAppService<
            FriendshipRequest,
            FriendshipRequestDetailDto,
            FriendshipRequestDto,
            Guid,
            FriendshipRequestGetListInput,
            FriendshipRequestCreateInput,
            FriendshipRequestUpdateInput>,
        IFriendshipRequestAppService
    {
        public FriendshipRequestAppService(IRepository<FriendshipRequest, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<FriendshipRequest>> CreateFilteredQueryAsync(FriendshipRequestGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))

                ;
        }
    }
}
