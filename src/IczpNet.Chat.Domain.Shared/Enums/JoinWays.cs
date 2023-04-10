using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 加入渠道
    /// </summary>
    public enum JoinWays
    {
        /// <summary>
        /// 常规方式
        /// </summary>
        [Description("常规方式")]
        Normal = 0,
        /// <summary>
        /// 朋友邀请
        /// </summary>
        [Description("朋友邀请")]
        Invitation = 1,
        /// <summary>
        /// 创建人
        /// </summary>
        [Description("创建人")]
        Creator = 2,
        /// <summary>
        /// 扫码
        /// </summary>
        [Description("扫码")]
        Scan = 3,
        /// <summary>
        /// 系统
        /// </summary>
        [Description("系统")]
        System = 4,
        /// <summary>
        /// 自动加入
        /// </summary>
        [Description("自动加入")]
        AutoJoin = 5,
    }
}
