using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialMembers.Dtos;

public class OfficialMemberGetListInput : BaseGetListInput
{
    public virtual Guid? OfficialId { get; set; }
    public virtual Guid? OwnerId { get; set; }

}
