using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberGetListInput : GetListInput
{
    /// <summary>
    /// SessionUnitId
    /// </summary>
    [Required]
    public Guid UnitId { get; set; }

    /// <summary>
    /// 是否创建者（群主）
    /// </summary>
    public bool? IsCreator { get; set; }

    /// <summary>
    /// 是否非公开
    /// </summary>
    public bool? IsPrivate { get; set; }

    /// <summary>
    /// 是否固定成员
    /// </summary>
    public bool? IsStatic { get; set; }

    /// <summary>
    /// 所属类型:
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
    public ChatObjectTypeEnums? OwnerObjectType { get; set; }

    /// <summary>
    /// 所属成员Id
    /// </summary>
    public long? OwnerId { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MinScore { get; set; }

    /// <summary>
    /// 最小Ticks
    /// </summary>
    public double? MaxScore { get; set; }

}
