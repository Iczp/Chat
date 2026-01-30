namespace IczpNet.Chat.SessionPermissions;

public class PermissionGrantValue
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 授予值
    /// </summary>
    public virtual long Value { get; set; }
}
