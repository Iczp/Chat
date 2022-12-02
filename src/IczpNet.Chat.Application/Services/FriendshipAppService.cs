using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Friendships.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
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
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.FriendId.HasValue, x => x.FriendId == input.FriendId)
                .WhereIf(input.IsCantacts.HasValue, x => x.IsCantacts == input.IsCantacts)
                .WhereIf(input.IsPassive.HasValue, x => x.IsPassive == input.IsPassive)
                .WhereIf(input.IsImmersed.HasValue, x => x.IsImmersed == input.IsImmersed)
                .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime >= input.StartCreationTime)
                .WhereIf(input.StartCreationTime.HasValue, x => x.CreationTime < input.EndCreationTime)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Rename.Contains(input.Keyword) || x.Remarks.Contains(input.Keyword))
                ;
        }

        [RemoteService(false)]
        public override Task<FriendshipDetailDto> UpdateAsync(Guid id, FriendshipUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }

        [RemoteService(false)]
        public override Task DeleteManyAsync(List<Guid> idList)
        {
            return base.DeleteManyAsync(idList);
        }

        [RemoteService(false)]
        public override Task DeleteAsync(Guid id)
        {
            return base.DeleteAsync(id);
        }
    }
}
