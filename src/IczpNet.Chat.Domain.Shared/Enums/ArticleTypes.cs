using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 文章类型,
    /// 0:常规
    /// </summary>
    [Description("文章类型")]
    public enum ArticleTypes : int
    {
        /// <summary>
        /// 文章类型（用户群 User）
        /// </summary>
        [Description("常规")]
        Normal = 0,
    }
}
