using System.Collections.Generic;
using System;

namespace IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;

public class OfficialGroupCreateInput : OfficialGroupUpdateInput
{
    public virtual Guid OfficialId { get; set; }

    public override string Name { get; set; }

    public override string Description { get; set; }

    public virtual List<Guid> ChatObjectIdList { get; set; }

}
