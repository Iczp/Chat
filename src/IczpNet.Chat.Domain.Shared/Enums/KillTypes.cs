using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 删除渠道
    /// </summary>
    [Description("删除渠道")]
    public enum KillTypes
    {
        /// <summary>
        /// 主动退出
        /// </summary>
        [Description("主动退出")]
        OwnerQuit = 0,

        /// <summary>
        /// 管理员移出
        /// </summary>
        [Description("管理员移出")]
        ManagerKill = 1,

        /// <summary>
        /// 系统移出
        /// </summary>
        [Description("系统移出")]
        SystemKill = 2,
    }
}
