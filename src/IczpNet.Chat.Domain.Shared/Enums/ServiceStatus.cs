using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 客服状态
    /// </summary>
    [Description("客服状态")]
    public enum ServiceStatus
    {
        /// <summary>
        /// 常规
        /// </summary>
        [Description("常规")]
        Normal = 0,
        /// <summary>
        /// 挂起
        /// </summary>
        [Description("挂起")]
        Pending = 1,
    }
}
