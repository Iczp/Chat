using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.SquareSections.Squares.Dtos;

public class SquareUpdateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
