using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SquareSections.Squares.Dtos;

public class SquareDto : BaseDto<Guid>
{
    public virtual long AutoId { get; set; }

    public virtual Guid? CategoryId { get; set; }

    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual SquareTypes Type { get; set; }

    public virtual int MemberCount { get; set; }

}
