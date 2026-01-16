using System.ComponentModel;

namespace IczpNet.Chat.Enums;

/// <summary>
/// 聊天对象类型:个人|群|服务号等
/// - 0=Anonymous:匿名
/// - 1=Personal:个人
/// - 2=Room:群
/// - 3=Official:服务号
/// - 4=Subscription:订阅号
/// - 5=Square:广场
/// - 6=Robot:机器人
/// - 7=ShopKeeper:掌柜
/// - 8=ShopWaiter:店小二
/// - 9=Customer:客户
/// </summary>
[Description("聊天对象类型")]
public enum ChatObjectTypeEnums
{
    /// <summary>
    /// 匿名
    /// </summary>
    [Description("匿名")]
    Anonymous =0,

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
    /// 服务号
    /// </summary>
    [Description("服务号")]
    Official = 3,

    /// <summary>
    /// 订阅号
    /// </summary>
    [Description("订阅号")]
    Subscription = 4,

    /// <summary>
    /// 广场
    /// </summary>
    [Description("广场")]
    Square = 5,

    /// <summary>
    /// 机器人
    /// </summary>
    [Description("机器人")]
    Robot = 6,

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
    Customer = 9,
}
