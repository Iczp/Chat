using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialGroupMembers.Dtos;

public class OfficialGroupMemberGetListInput : BaseGetListInput
{
    public virtual Guid? OwnerId { get; set; }

}
