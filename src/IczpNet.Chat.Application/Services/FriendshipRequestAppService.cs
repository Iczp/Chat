using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.FriendshipRequests;
using IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp;
using System.Collections.Generic;

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

        [RemoteService(false)]
        public override Task<FriendshipRequestDetailDto> UpdateAsync(Guid id, FriendshipRequestUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }

        [RemoteService(false)]
        public override Task DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }

        [RemoteService(false)]
        public override Task DeleteManyAsync(List<Guid> idList)
        {
            return base.DeleteManyAsync(idList);
        }
    }
}
