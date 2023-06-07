using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.EntryNames.Dtos;

public class EntryNameGetListInput : BaseGetListInput
{
    public virtual bool? IsChoice { get; set; }

    public virtual bool? IsUniqued { get; set; } = false;

    public virtual bool? IsRequired { get; set; }

    public virtual bool? IsStatic { get; set; }

    public virtual bool? IsPublic { get; set; }
}
