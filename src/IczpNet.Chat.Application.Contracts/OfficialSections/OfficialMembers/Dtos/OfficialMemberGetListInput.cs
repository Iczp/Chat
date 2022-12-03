using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialMembers.Dtos;

public class OfficialMemberGetListInput : BaseGetListInput
{
    public virtual Guid? OwnerId { get; set; }

}
