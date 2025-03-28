﻿using IczpNet.Chat.Management.BaseDtos;

namespace IczpNet.Chat.Management.OfficialSections.Officials.Dtos;

public class OfficialCreateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    public virtual string Portrait { get; set; }

    public virtual string Description { get; set; }

}
