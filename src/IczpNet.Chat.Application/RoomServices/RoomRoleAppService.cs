using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.RoomServices
{
    public class RoomRoleAppService
        : CrudChatAppService<
            RoomRole,
            RoomRoleDetailDto,
            RoomRoleDto,
            Guid,
            RoomRoleGetListInput,
            RoomRoleCreateInput,
            RoomRoleUpdateInput>,
        IRoomRoleAppService
    {
        public RoomRoleAppService(IRepository<RoomRole, Guid> repository) : base(repository)
        {
        }

        protected override async Task<IQueryable<RoomRole>> CreateFilteredQueryAsync(RoomRoleGetListInput input)
        {
            return (await base.CreateFilteredQueryAsync(input))
                //.WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(!input.IsNullRoomId.HasValue && input.RoomId.HasValue, x => x.RoomId == input.RoomId)
                .WhereIf(input.IsNullRoomId.HasValue, x => x.RoomId == null)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword) || x.Description.Contains(input.Keyword))
                ;
        }

        protected override async Task CheckCreateAsync(RoomRoleCreateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => x.RoomId == input.RoomId && x.Name == input.Name), $"Already exists room role name:{input.Name}");
            await base.CheckCreateAsync(input);
        }

        protected override Task<RoomRole> MapToEntityAsync(RoomRoleCreateInput createInput)
        {
            return Task.FromResult(new RoomRole(GuidGenerator.Create(), createInput.RoomId, createInput.Name, createInput.Code, createInput.Description));
        }

        protected override Task SetUpdateEntityAsync(RoomRole entity, RoomRoleUpdateInput input)
        {
            entity.SetName(input.Name);
            return base.SetUpdateEntityAsync(entity, input);
        }

        protected override async Task CheckUpdateAsync(Guid id, RoomRole entity, RoomRoleUpdateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => !x.Id.Equals(id) && x.Name.Equals(input.Name)), $"Already exists room role name:{input.Name}");

            await base.CheckUpdateAsync(id, entity, input);
        }

        protected override Task CheckDeleteAsync(RoomRole entity)
        {
            var memberCount = entity.MemberList.Count;

            Assert.If(memberCount != 0, $"role's member count: memberCount");

            return base.CheckDeleteAsync(entity);
        }
    }
}
