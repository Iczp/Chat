using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.OfficialSections.Officials.Dtos;

public class OfficialUpdateInput : BaseInput, IName
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Description { get; set; }
}
