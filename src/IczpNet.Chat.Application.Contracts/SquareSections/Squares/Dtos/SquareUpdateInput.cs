using IczpNet.Chat.BaseDtos;
using System.Collections.Generic;
using System;

namespace IczpNet.Chat.SquareSections.Squares.Dtos;

public class SquareUpdateInput : BaseInput
{
    public virtual Guid? CategoryId { get; set; }

    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
