using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    public enum ChatObjectTypes
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
        /// 掌柜
        /// </summary>
        [Description("掌柜")]
        ShopKeeper = 7,
        /// <summary>
        /// 店小二
        /// </summary>
        [Description("店小二")]
        ShopWaiter = 8,
        /// <summary>
        /// 客户
        /// </summary>
        [Description("客户")]
        Customer = 8,
    }
}
