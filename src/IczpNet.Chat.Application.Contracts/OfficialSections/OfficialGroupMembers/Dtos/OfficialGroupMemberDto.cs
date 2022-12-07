using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialGroupMembers.Dtos;

public class OfficialGroupMemberDto : BaseDto<Guid>
{
    public virtual ChatObjectSimpleDto Owner { get; set; }
}
