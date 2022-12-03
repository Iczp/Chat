using IczpNet.Chat.RoomSections.RoomRoles.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.RoomSections.RoomRoles;

public interface IRoomRoleAppService :
    ICrudAppService<
        RoomRoleDetailDto,
        RoomRoleDto,
        Guid,
        RoomRoleGetListInput,
        RoomRoleCreateInput,
        RoomRoleUpdateInput>
{
}
