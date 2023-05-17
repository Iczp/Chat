using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 性别
    /// </summary>
    [Description("性别")]
    public enum Genders
    {
        /// <summary>
        /// 保密
        /// </summary>
        [Description("保密")]
        Neutral = 0,
        /// <summary>
        /// 男
        /// </summary>
        [Description("男")]
        Male = 1,
        /// <summary>
        /// 女
        /// </summary>
        [Description("女")]
        Female = 2,
    }
}
