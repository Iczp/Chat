using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;

public class OfficialGroupGetListInput : BaseGetListInput
{
    public virtual Guid? OfficialId { get; set; }

    public virtual bool? IsPublic { get; set; }
}
