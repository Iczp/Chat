using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;

public class OfficialGroupGetListInput : BaseGetListInput
{
    public virtual Guid? OwnerId { get; set; }

}
