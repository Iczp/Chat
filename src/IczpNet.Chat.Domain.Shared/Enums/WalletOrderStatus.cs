using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    [Description("订单状态")]
    public enum WalletOrderStatus
    {
        /// <summary>
        /// 处理中
        /// </summary>
        [Description("处理中")]
        Pending = 0,

        /// <summary>
        /// 收入
        /// </summary>
        [Description("成功")]
        Success = 1,

        /// <summary>
        /// 关闭
        /// </summary>
        [Description("关闭")]
        Close = 2,

        /// <summary>
        /// 无效
        /// </summary>
        [Description("无效")]
        Invalid = 3,

        /// <summary>
        /// 失败
        /// </summary>
        [Description("失败")]
        Fail = 4,

    }
}
