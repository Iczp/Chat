using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 提醒器类型
    /// </summary>
    public enum ReminderTypes
    {
        /// <summary>
        /// @你
        /// </summary>
        [Description("@你")]
        Normal = 0,
        /// <summary>
        /// 通知提醒
        /// </summary>
        [Description("通知提醒")]
        Notice = 1,
        /// <summary>
        /// 服务提醒
        /// </summary>
        [Description("服务提醒")]
        Service = 2,
    }
}
