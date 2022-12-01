using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.OfficialSections.Officials.Dtos;

public class OfficialDto : BaseDto<Guid>
{
    public virtual long AutoId { get; set; }

    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual OfficialTypeEnum Type { get; set; }

    public virtual int MemberCount { get; set; }

}
