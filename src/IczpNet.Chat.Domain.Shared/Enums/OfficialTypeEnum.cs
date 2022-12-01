using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 公众号类型
    /// </summary>
    public enum OfficialTypeEnum
    {
        /// <summary>
        /// 订阅号
        /// </summary>
        [Description("订阅号")]
        Subscription = 0,
        /// <summary>
        /// 服务号
        /// </summary>
        [Description("服务号")]
        Service = 1,
    }
}
