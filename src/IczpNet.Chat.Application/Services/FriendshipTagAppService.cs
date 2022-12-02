using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.FriendshipTagSections.FriendshipTags;
using IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;
using IczpNet.Chat.SessionSections.Friendships;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
{
    public class FriendshipTagAppService
        : CrudChatAppService<
            FriendshipTag,
            FriendshipTagDetailDto,
            FriendshipTagDto,
            Guid,
            FriendshipTagGetListInput,
            FriendshipTagCreateInput,
            FriendshipTagUpdateInput>,
        IFriendshipTagAppService
    {
        protected IRepository<ChatObject, Guid> ChatObjectRepository { get; }
        public FriendshipTagAppService(
            IRepository<FriendshipTag, Guid> repository,
            IRepository<ChatObject, Guid> chatObjectRepository) : base(repository)
        {
            ChatObjectRepository = chatObjectRepository;
        }

        protected override async Task<IQueryable<FriendshipTag>> CreateFilteredQueryAsync(FriendshipTagGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))

                ;
        }

        protected override async Task<FriendshipTag> MapToEntityAsync(FriendshipTagCreateInput createInput)
        {
            var owner = Assert.NotNull(await ChatObjectRepository.FindAsync(createInput.OwnerId), $"No such Entity by OwnerId:{createInput.OwnerId}.");

            Assert.If(await Repository.AnyAsync(x => x.OwnerId == createInput.OwnerId && x.Name == createInput.Name), $"Already exists [{createInput.Name}].");

            return new FriendshipTag(owner, createInput.Name);
        }

        [RemoteService(false)]
        public override Task<FriendshipTagDetailDto> UpdateAsync(Guid id, FriendshipTagUpdateInput input)
        {
            return base.UpdateAsync(id, input);
        }
    }
}
