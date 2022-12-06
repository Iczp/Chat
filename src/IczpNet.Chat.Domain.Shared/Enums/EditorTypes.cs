using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 编辑器类型,
    /// 0:常规
    /// 1:MarkDown
    /// </summary>
    public enum EditorTypes
    {
        /// <summary>
        /// 无
        /// </summary>
        [Description("无")]
        None = 0,
        /// <summary>
        /// MarkDown
        /// </summary>
        [Description("MarkDown")]
        MarkDown = 1,
    }
}
