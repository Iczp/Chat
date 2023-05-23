using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.Words.Dtos;

public class WordUpdateInput : BaseInput
{
    public virtual bool IsDirty { get; set; }
}
