using IczpNet.Chat.BaseDtos;
using System.Collections.Generic;
using System.ComponentModel;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;

public class SessionPermissionDefinitionGetListInput : GetListInput
{
    /// <summary>
    /// 分组Id
    /// </summary>
    [DefaultValue(null)]
    public virtual List<long> GroupIdList { get; set; }

    /// <summary>
    /// 是否包含子组分组
    /// </summary>
    public bool IsImportChildGroup { get; set; }
}
