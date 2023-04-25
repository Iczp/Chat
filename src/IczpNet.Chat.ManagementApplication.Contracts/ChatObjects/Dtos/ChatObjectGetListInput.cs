using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using IczpNet.Chat.Management.BaseDtos;

namespace IczpNet.Chat.Management.ChatObjects.Dtos;

public class ChatObjectGetListInput : BaseTreeGetListInput<long>
{
    public virtual string ChatObjectTypeId { get; set; }

    public virtual ChatObjectTypeEnums? ObjectType { get; set; }

    [DefaultValue(null)]
    public virtual List<Guid> CategoryIdList { get; set; }
    public bool IsImportChildCategory { get; set; }
}
