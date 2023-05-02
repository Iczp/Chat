using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectGetListInput : BaseTreeGetListInput<long>
{
    public virtual string ChatObjectTypeId { get; set; }

    public virtual bool? IsStatic { get; set; }

    public virtual bool? IsPublic { get; set; }

    public virtual bool? IsEnabled { get; set; }

    public virtual bool? IsDefault { get; set; }

    public virtual ChatObjectTypeEnums? ObjectType { get; set; }

    [DefaultValue(null)]
    public virtual List<Guid> CategoryIdList { get; set; }
    public bool IsImportChildCategory { get; set; }
    
}
