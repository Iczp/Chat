using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;

public class OfficialGroupDto : BaseDto<Guid>
{
    public virtual string Name { get; set; }

    public virtual string Description { get; set; }

    public virtual bool IsPublic { get; set; }

    public int GroupMemberCount { get; set; }
}
