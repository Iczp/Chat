using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    public enum MessageChannels
    {
        /// <summary>
        /// 个人
        /// </summary>
        [Description("个人")]
        PersonalChannel = 1,
        /// <summary>
        /// 普通群
        /// </summary>
        [Description("普通群")]
        RoomChannel = 2,
        /// <summary>
        /// 订阅号
        /// </summary>
        [Description("订阅号")]
        SubscriptionChannel = 3,
        /// <summary>
        /// 公众号-服务号
        /// </summary>
        [Description("服务号")]
        ServiceChannel = 4,
        /// <summary>
        /// 广场
        /// </summary>
        [Description("广场")]
        SquareChannel = 5,
        /// <summary>
        /// 机器人
        /// </summary>
        [Description("机器人")]
        RobotChannel = 6,
        /// <summary>
        /// 电子商务
        /// </summary>
        [Description("电子商务")]
        ElectronicCommerceChannel = 7,
    }
}
