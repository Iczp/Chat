using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    public enum ChatObjectType
    {
        /// <summary>
        /// 个人
        /// </summary>
        [Description("个人")]
        Personal = 1,
        /// <summary>
        /// 群
        /// </summary>
        [Description("群")]
        Room = 2,
        /// <summary>
        /// 公众号
        /// </summary>
        [Description("公众号")]
        Official = 3,
        /// <summary>
        /// 广场
        /// </summary>
        [Description("广场")]
        Square = 4,
        /// <summary>
        /// 机器人
        /// </summary>
        [Description("机器人")]
        Robot = 5,
        /// <summary>
        /// 电子商务
        /// </summary>
        [Description("电子商务")]
        ElectronicCommerce = 7,
    }
}
