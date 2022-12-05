using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.SessionSections.Friendships;
using IczpNet.Chat.SessionSections.Friendships.Dtos;
using Microsoft.AspNetCore.Mvc;
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
        protected IRepository<FriendshipTag, Guid> FriendshipTagRepository { get; }

        public FriendshipAppService(
            IRepository<Friendship, Guid> repository,
            IRepository<FriendshipTag, Guid> friendshipTagRepository) : base(repository)
        {
            FriendshipTagRepository = friendshipTagRepository;
        }


        protected override async Task<IQueryable<Friendship>> CreateFilteredQueryAsync(FriendshipGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.TagIdList.IsAny(), x => x.TagList.Any(d => input.TagIdList.Contains(d.FriendshipTagId)))
                .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
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

        protected virtual async Task<Friendship> UpdateAsync(Guid friendshipId, Action<Friendship> action)
        {
            var entity = await Repository.GetAsync(friendshipId);

            action?.Invoke(entity);

            return await Repository.UpdateAsync(entity, autoSave: true);
        }

        [HttpPost]
        public async Task<string> SetRenameAsync(Guid friendshipId, string rename)
        {
            return (await UpdateAsync(friendshipId, x => x.Rename = rename)).Rename;
        }

        [HttpPost]
        public async Task<string> SetRemarksAsync(Guid friendshipId, string remarks)
        {
            return (await UpdateAsync(friendshipId, x => x.Remarks = remarks)).Remarks;
        }

        [HttpPost]
        public async Task<bool> SetIsCantactsAsync(Guid friendshipId, bool isCantacts)
        {
            return (await UpdateAsync(friendshipId, x => x.IsCantacts = isCantacts)).IsCantacts;
        }

        [HttpPost]
        public async Task<bool> SetIsImmersedAsync(Guid friendshipId, bool isImmersed)
        {
            return (await UpdateAsync(friendshipId, x => x.IsImmersed = isImmersed)).IsImmersed;
        }

        [HttpPost]

        public async Task<bool> SetIsShowMemberNameAsync(Guid friendshipId, bool isShowMemberName)
        {
            return (await UpdateAsync(friendshipId, x => x.IsShowMemberName = isShowMemberName)).IsShowMemberName;
        }

        [HttpPost]
        public async Task<bool> SetIsShowReadNameAsync(Guid friendshipId, bool isShowRead)
        {
            return (await UpdateAsync(friendshipId, x => x.IsShowRead = isShowRead)).IsShowRead;
        }

        //[RemoteService(false)]
        [HttpPost]
        public async Task<string> SetBackgroundImageAsync(Guid friendshipId, string backgroundImage)
        {
            return (await UpdateAsync(friendshipId, x => x.BackgroundImage = backgroundImage)).BackgroundImage;
        }

        [HttpPost]
        public async Task<List<Guid>> SetTagListAsync(Guid friendshipId, List<Guid> tagIdList)
        {
            var entity = await Repository.GetAsync(friendshipId);

            var tagList = new List<FriendshipTag>();

            foreach (var tagId in tagIdList)
            {
                var tag = await FriendshipTagRepository.GetAsync(tagId);

                Assert.If(tag.OwnerId != entity.OwnerId, $"Cannot be used tagId:{tagId}");

                tagList.Add(tag);
            }

            entity.SetTabList(tagList);

            return tagIdList;
        }
    }
}
