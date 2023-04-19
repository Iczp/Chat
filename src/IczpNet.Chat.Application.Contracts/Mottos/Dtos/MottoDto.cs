using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.Mottos.Dtos;

public class MottoDto : BaseDto<Guid>
{
    public virtual string Title { get; set; }

    public virtual long OwnerId { get; set; }
}
