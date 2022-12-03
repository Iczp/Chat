using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
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
                //.WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                ;
        }
    }
}
