using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 通知类型,
    /// 0:普通通知
    /// 1:延时通知
    /// 2:规律性通知
    /// </summary>
    [Description("通知类型")]
    public enum NoticeTypes : int
    {
        /// <summary>
        /// 普通通知
        /// </summary>
        [Description("Immediate")]
        Immediate = 0,
        /// <summary>
        /// 延时通知
        /// </summary>
        [Description("Reminder")]
        Delay = 1,
        /// <summary>
        /// 规律性通知
        /// </summary>
        [Description("Regular")]
        Regular = 2
    }
}
