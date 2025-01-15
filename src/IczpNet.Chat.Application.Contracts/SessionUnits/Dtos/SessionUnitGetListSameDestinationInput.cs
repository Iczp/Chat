using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitGetListSameDestinationInput : GetListInput
{
    /// <summary>
    /// 原聊天对象Id
    /// </summary>
    [Required]
    public virtual long SourceId { get; set; }

    /// <summary>
    /// 目标对象Id
    /// </summary>
    [Required]
    public virtual long TargetId { get; set; }

    /// <summary>
    /// 目标聊天对象类型
    /// </summary>
    [DefaultValue(null)]
    public virtual List<ChatObjectTypeEnums> ObjectTypeList { get; set; }
}