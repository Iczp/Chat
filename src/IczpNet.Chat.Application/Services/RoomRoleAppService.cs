using IczpNet.AbpCommons;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Services
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
                ;
        }

        protected override async Task<RoomRole> MapToEntityAsync(RoomRoleCreateInput createInput)
        {

            Assert.If(await Repository.AnyAsync(x => x.RoomId == createInput.RoomId && x.Name == createInput.Name), $"Already exists room role name:{createInput.Name}");

            return new RoomRole(GuidGenerator.Create(), createInput.RoomId, createInput.Name, createInput.Code, createInput.Description);
        }

        protected override Task SetUpdateEntityAsync(RoomRole entity, RoomRoleUpdateInput input)
        {
            entity.SetName(input.Name);
            return base.SetUpdateEntityAsync(entity, input);
        }

        [HttpPost]
        public override async Task<RoomRoleDetailDto> UpdateAsync(Guid id, RoomRoleUpdateInput input)
        {
            Assert.If(await Repository.AnyAsync(x => !x.Id.Equals(id) && x.Name.Equals(input.Name)), $"Already exists room role name:{input.Name}");

            return await base.UpdateAsync(id, input);
        }

        [HttpPost]
        public override async Task DeleteAsync(Guid id)
        {
            await CheckDeletePolicyAsync();

            var entity = await GetEntityByIdAsync(id);

            await CheckDeleteAsync(entity);

            await DeleteByIdAsync(id);
        }

        protected virtual async Task CheckDeleteAsync(RoomRole entity)
        {
            await CheckDeleteIsStaticAsync(entity.Id);
        }
    }
}
