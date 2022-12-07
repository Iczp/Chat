using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialGroupMembers.Dtos;

public class OfficialGroupMemberGetListInput : BaseGetListInput
{


    public virtual Guid? OfficialGroupId { get; set; }
    public virtual Guid? OwnerId { get; set; }
}
