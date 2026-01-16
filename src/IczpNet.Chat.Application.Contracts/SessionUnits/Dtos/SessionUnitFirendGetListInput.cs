using IczpNet.Chat.Enums;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitFirendGetListInput : SessionUnitLatestGetListInput
{


    /// <summary>
    /// 好友归档
    /// - 0=All:全部,
    /// - 1=Pinned:置顶,
    /// - 2=Following:关注,
    /// - 3=RemindAll:@所有人,
    /// - 4=RemindMe:@我,
    /// - 5=Immersed:静默,
    /// - 6=Creator:创建人,
    /// - 7=HasBadge:有未读消息
    /// </summary>
    public FriendTypes FriendType { get; set; }

    /// <summary>
    /// 是否在线
    /// </summary>
    public bool? IsOnline { get; set; }

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
    public ChatObjectTypeEnums? DestinationType { get; set; }


}
