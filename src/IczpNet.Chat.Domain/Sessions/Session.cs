using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionRoles;
using IczpNet.Chat.SessionTags;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Sessions;

[Index(nameof(LastMessageId), AllDescending = true)]
[Index(nameof(SessionKey))]
[Index(nameof(CreationTime), AllDescending = true)]
[Index(nameof(MessageTotalCount), AllDescending = true)]
[Index(nameof(MessageTotalCountUpdateTime), AllDescending = true)]
public class Session : BaseEntity<Guid>, IChatOwner<long?>, IIsEnabled
{
    /// <summary>
    /// 
    /// </summary>
    [StringLength(80)]
    [Comment("SessionKey")]
    public virtual string SessionKey { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    //[StringLength(36)]
    [Comment("Channel")]
    public virtual Channels Channel { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [StringLength(50)]
    public virtual string Title { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [StringLength(100)]
    [Comment("Description")]
    public virtual string Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long? OwnerId { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    [Comment("Owner")]
    public virtual ChatObject Owner { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsEnabled { get; protected set; }

    /// <summary>
    /// 第一条消息
    /// </summary>
    public virtual long? FirstMessageId { get; protected set; }

    /// <summary>
    /// 第一条消息
    /// </summary>
    [ForeignKey(nameof(FirstMessageId))]
    public virtual Message FirstMessage { get; protected set; }

    /// <summary>
    /// 最新消息
    /// </summary>
    public virtual long? LastMessageId { get; protected set; }

    /// <summary>
    /// 最新消息
    /// </summary>
    [ForeignKey(nameof(LastMessageId))]
    public virtual Message LastMessage { get; protected set; }

    /// <summary>
    /// 当前会话最后分配的消息序号 SessionSequence
    /// </summary>
    [Comment("当前会话最后分配的消息序号")]
    public virtual long LastMessageSeq { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [Comment("是否可以设置为'免打扰'")]
    public virtual bool IsEnableSetImmersed { get; protected set; } = true;

    /// <summary>
    /// 消息总数量
    /// </summary>
    [Comment("消息总数量")]
    public virtual int MessageTotalCount { get; protected set; }

    /// <summary>
    /// 更新消息总数量时间
    /// </summary>
    [Comment("更新消息总数量时间")]
    public virtual DateTime? MessageTotalCountUpdateTime { get; protected set; }

    /// <summary>
    /// 全员禁言（群主和管理员除外）
    /// </summary>
    [Comment("全员禁言")]
    public virtual bool IsAllMuted { get; protected set; } = false;

    /// <summary>
    /// 群邀请策略
    /// </summary>
    [Comment("群邀请策略")]
    public virtual RoomInvitePolicies InvitePolicy { get; protected set; } = RoomInvitePolicies.AllMembers;

    /// <summary>
    /// 允许的入群渠道（位掩码）
    /// </summary>
    [Comment("允许的入群渠道")]
    public virtual RoomJoinChannels AllowJoinChannels { get; protected set; } = RoomJoinChannels.All;

    [InverseProperty(nameof(Message.Session))]
    public virtual IList<Message> MessageList { get; internal set; } = [];

    public virtual IList<SessionUnit> UnitList { get; internal set; } = [];

    public virtual IList<SessionTag> TagList { get; protected set; } = [];

    public virtual IList<SessionRole> RoleList { get; protected set; } = [];

    [NotMapped]
    public virtual int? MemberCount { get; set; } //=> GetMemberCount();

    [NotMapped]
    public virtual int? MessageCount { get; set; } //=> MessageList.Count;

    [NotMapped]
    public virtual int? TagCount { get; set; } // => TagList.Count;

    [NotMapped]
    public virtual int? RoleCount { get; set; } //=> RoleList.Count;

    protected Session() { }

    public Session(Guid id, string sessionKey, Channels channel) : base(id)
    {
        SessionKey = sessionKey;
        Channel = channel;
    }

    //internal virtual int GetMemberCount()
    //{
    //    return UnitList.Count();
    //}

    internal virtual void SetUnitList(List<SessionUnit> unitList)
    {
        UnitList = unitList;
    }

    public virtual SessionUnit AddSessionUnit(SessionUnit sessionUnit)
    {
        UnitList.Add(sessionUnit);
        return sessionUnit;
    }

    internal void SetOwner(ChatObject chatObject)
    {
        Owner = chatObject;
        OwnerId = chatObject.Id;
    }

    internal void SetMessageList(List<Message> messages)
    {
        MessageList = messages;
    }

    internal SessionTag AddTag(SessionTag sessionTag)
    {
        TagList.Add(sessionTag);
        return sessionTag;
    }

    internal SessionRole AddRole(SessionRole sessionRole)
    {
        RoleList.Add(sessionRole);
        return sessionRole;
    }

    public void SetLastMessage(Message lastMessage)
    {
        LastMessage = lastMessage;
        LastMessageId = lastMessage.Id;
    }

    public virtual void SetRoomPolicies(
        bool? isAllMuted = null,
        RoomInvitePolicies? invitePolicy = null,
        RoomJoinChannels? allowJoinChannels = null)
    {
        if (isAllMuted.HasValue)
        {
            IsAllMuted = isAllMuted.Value;
        }
        if (invitePolicy.HasValue)
        {
            InvitePolicy = invitePolicy.Value;
        }
        if (allowJoinChannels.HasValue)
        {
            AllowJoinChannels = allowJoinChannels.Value;
        }
    }

    public virtual void SetIsEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
