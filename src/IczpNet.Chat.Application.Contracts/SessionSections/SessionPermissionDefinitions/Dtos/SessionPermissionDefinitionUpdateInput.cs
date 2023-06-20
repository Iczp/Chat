using IczpNet.Chat.BaseDtos;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions.Dtos;

public class SessionPermissionDefinitionUpdateInput : BaseInput
{
    /// <summary>
    /// 权限分组
    /// </summary>
    [DefaultValue(null)]
    public virtual long? GroupId { get; set; }

    /// <summary>
    /// 权限名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    [StringLength(200)]
    public virtual string Description { get; set; }

    //[StringLength(50)]
    //public virtual string DateType { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    public virtual long DefaultValue { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public virtual long MaxValue { get; set; }

    /// <summary>
    /// 最小值
    /// </summary>
    public virtual long MinValue { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public virtual long Sorting { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; set; }
}
