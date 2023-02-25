using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.RoomForbiddenMembers;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;


namespace IczpNet.Chat.RoomSections.Rooms;

public class Room : ChatObject, IChatOwner<Guid?>
{
    public const ChatObjectTypeEnums ChatObjectTypeValue = ChatObjectTypeEnums.Room;

    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; protected set; }

    /// <summary>
    /// 会话
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

    public virtual int MemberCount { get; protected set; }

    /// <summary>
    /// 群拥有者 OwnerUserId (群主)
    /// </summary>
    public virtual Guid? OwnerId { get; protected set; }

    /// <summary>
    /// 群主
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }
    /// <summary>
    /// 群类型（自由群、职位群）
    /// </summary>
    public virtual RoomTypes Type { get; set; }

    /// <summary>
    /// 成员名称的显示方式
    /// </summary>
    public virtual MemberNameDisplayModes MemberNameDisplayMode { get; protected set; }

    /// <summary>
    /// 头像的显示方式
    /// </summary>
    public virtual PortraitDisplayModes PortraitDisplayMode { get; protected set; }

    /// <summary>
    /// 聊天背景，默认为 null
    /// </summary>
    [StringLength(300)]
    [MaxLength(300)]
    public virtual string BackgroundImage { get; set; }

    /// <summary>
    /// 默认群角色
    /// </summary>
    public virtual Guid? DefaultRoleId { get; protected set; }

    /// <summary>
    /// 入群邀请方式
    /// </summary>
    public virtual InvitationMethods InvitationMethod { get; set; }

    /// <summary>
    /// 是否可以设置消息免打扰
    /// </summary>
    public virtual bool IsCanSetImmersed { get; set; }

    /// <summary>
    /// 是否可以设置聊天背景
    /// </summary>
    public virtual bool IsCanSetBackground { get; set; }

    /// <summary>
    /// 全体禁言（除群主及管理员）
    /// </summary>
    public virtual bool IsForbiddenAll { get; set; } = false;

    /// <summary>
    /// 是否允许自动加入群（当不是群成员的时） Allow to automatically join the room(when not a member of the room)
    /// 在进群聊天时，还不是群成的时，允许自动加入群（课程群有这个需求）
    /// </summary>
    public virtual bool IsAllowAutoJoin { get; set; } = false;

    /// <summary>
    /// 群成员
    /// </summary>
    public virtual IList<RoomMember> RoomMemberList { get; protected set; } = new List<RoomMember>();

    /// <summary>
    /// 被禁言的成员
    /// </summary>
    public virtual IList<RoomForbiddenMember> RoomForbiddenMemberList { get; set; }

    /// <summary>
    /// 默认群角色
    /// </summary>
    [ForeignKey(nameof(DefaultRoleId))]
    public virtual RoomRole DefaultRole { get; set; }

    /// <summary>
    /// 群角色
    /// </summary>
    public virtual IList<RoomRole> RoleList { get; set; }

    protected Room()
    {
        ObjectType = ChatObjectTypeValue;
    }

    public Room(Guid id, string name, string code, string description, Guid? ownerId) : base(id, ChatObjectTypeValue)
    {
        SetName(name);
        Code = code;
        Description = description;
        Type = RoomTypes.Normal;
        OwnerId = ownerId;
    }

    public int? GetMemberCount()
    {
        return Session?.GetMemberCount();
    }

    internal void SetMemberCount(int count)
    {
        MemberCount = count;
    }

    internal void SetOwner(ChatObject chatObject)
    {
        Owner = chatObject;
    }

    internal void SetSession(Session session)
    {
        Session = session;
    }

    public bool IsInRoom(ChatObject member)
    {
        if (Session == null)
        {
            return false;
        }
        return Session.UnitList.Any(x => x.OwnerId == member.Id);
    }
}
