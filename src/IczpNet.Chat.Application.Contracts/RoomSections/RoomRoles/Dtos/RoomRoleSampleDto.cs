using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RoomSections.RoomRoles.Dtos;

public class RoomRoleSampleDto : EntityDto<Guid>
{
    public virtual string Name { get; set; }
}
