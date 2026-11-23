using System.ComponentModel;

namespace IczpNet.Chat.MessageTemplates;

/// <summary>
/// 模板可见性
/// </summary>
public enum MessageTemplateVisibility
{
    /// <summary>
    /// 仅所属账号私有可用
    /// </summary>
    [Description("私有")]
    Private = 0,

    /// <summary>
    /// 租户内具备使用权限的账号共享
    /// </summary>
    [Description("租户共享")]
    TenantShared = 1,
}
