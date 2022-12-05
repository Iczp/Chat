using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.RoomSections.RoomForbiddenMembers;
using IczpNet.Chat.RoomSections.RoomMembers;
using IczpNet.Chat.RoomSections.RoomRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.RoomSections.Rooms;

public class Room : ChatObject, IChatOwner<Guid?>
{
    public const ChatObjectTypeEnum ChatObjectTypeValue = ChatObjectTypeEnum.Room;

    /// <summary>
    /// 群拥有者 OwnerUserId (群主)
    /// </summary>
    public virtual Guid? OwnerId { get; protected set; }

    /// <summary>
    /// 群类型（自由群、职位群）
    /// </summary>
    public virtual RoomTypeEnum Type { get; set; }

    /// <summary>
    /// 成员名称的显示方式
    /// </summary>
    public virtual MemberNameDisplayModeEnum MemberNameDisplayMode { get; protected set; }

    /// <summary>
    /// 头像的显示方式
    /// </summary>
    public virtual PortraitDisplayModeEnum PortraitDisplayMode { get; protected set; }

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
    public virtual InvitationMethodEnum InvitationMethod { get; set; }

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
    /// 群主
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }

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

    public Room(Guid id, string name, string code, string description, IList<RoomMember> roomMemberList, Guid? ownerId) : base(id, ChatObjectTypeValue)
    {
        SetName(name);
        Code = code;
        Description = description;
        OwnerId = ownerId;
        Type = RoomTypeEnum.Normal;
        RoomMemberList = roomMemberList;
    }

    public int GetMemberCount()
    {
        return RoomMemberList.Count;
    }
}
