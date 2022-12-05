using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;

public class OfficialGroupUpdateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Description { get; set; }
}
