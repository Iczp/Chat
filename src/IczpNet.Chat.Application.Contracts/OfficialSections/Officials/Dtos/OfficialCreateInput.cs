using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.OfficialSections.Officials.Dtos;

public class OfficialCreateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }

}
