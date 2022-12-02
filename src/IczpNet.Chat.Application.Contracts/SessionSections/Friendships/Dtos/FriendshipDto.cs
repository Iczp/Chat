using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.SessionSections.Friendships.Dtos;

public class FriendshipDto : BaseDto<Guid>
{
    public virtual Guid OwnerId { get; set; }

    //public virtual Guid? FriendId { get;  set; }

    //public virtual ChatObjectSimpleDto Owner { get; set; }

    public virtual ChatObjectSimpleDto Friend { get; set; }

    /// <summary>
    /// 备注名称
    /// </summary>
    public virtual string Rename { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public virtual string Remarks { get; set; }

    /// <summary>
    /// 是否保存通讯录(群)
    /// </summary>
    public virtual bool IsCantacts { get; set; }

    /// <summary>
    /// 消息免打扰，默认为 false
    /// </summary>
    public virtual bool IsImmersed { get; set; }

    /// <summary>
    /// 是否显示成员名称
    /// </summary>
    public virtual bool IsShowMemberName { get; set; }

    /// <summary>
    /// 是否显示已读
    /// </summary>
    public virtual bool IsShowRead { get; set; }

    /// <summary>
    /// 聊天背景，默认为 null
    /// </summary>
    public virtual string BackgroundImage { get; set; }

    /// <summary>
    /// 是否被动的（主动添加为0,被动添加为1）
    /// </summary>
    public virtual bool IsPassive { get; set; }

}
