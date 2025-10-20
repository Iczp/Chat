using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.DeletedRecorders;
using IczpNet.Chat.Enums;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.Follows;
using IczpNet.Chat.MessageSections.MessageFollowers;
using IczpNet.Chat.MessageSections.MessageReminders;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.OpenedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionSections;
using IczpNet.Chat.SessionSections.SessionPermissionUnitGrants;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnitContactTags;
using IczpNet.Chat.SessionSections.SessionUnitCounters;
using IczpNet.Chat.SessionSections.SessionUnitEntryValues;
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using IczpNet.Chat.SessionUnitSettings;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.SimpleStateChecking;

namespace IczpNet.Chat.SessionUnits;

[Index(nameof(Sorting), AllDescending = true)]
[Index(nameof(Ticks), AllDescending = true)]
[Index(nameof(LastMessageId), IsDescending = new[] { true })]
[Index(nameof(Sorting), nameof(Ticks), AllDescending = true)]

[Index(nameof(Key), AllDescending = true)]
[Index(nameof(DestinationObjectType), AllDescending = true)]
[Index(nameof(OwnerObjectType), AllDescending = true)]
[Index(nameof(IsDeleted))]
[Index(nameof(CreationTime), AllDescending = true)]

//OwnerId
[Index(nameof(OwnerId), AllDescending = true)]

[Index(nameof(OwnerId), nameof(PublicBadge), nameof(PrivateBadge), AllDescending = true)]

//LastMessageId
[Index(nameof(OwnerId), nameof(Sorting), nameof(LastMessageId), nameof(IsDeleted), AllDescending = true)]
[Index(nameof(OwnerId), nameof(Sorting), nameof(LastMessageId), nameof(IsDeleted), IsDescending = [true, true, false, true], Name = $"IX_Chat_SessionUnit_${nameof(OwnerId)}_${nameof(Sorting)}_Desc_${nameof(LastMessageId)}_Asc")]

//Ticks
[Index(nameof(OwnerId), nameof(Sorting), nameof(Ticks), nameof(IsDeleted), AllDescending = true)]
[Index(nameof(OwnerId), nameof(Sorting), nameof(Ticks), nameof(IsDeleted), IsDescending = [true, true, false, true], Name = $"IX_Chat_SessionUnit_${nameof(OwnerId)}_${nameof(Sorting)}_Desc_{nameof(Ticks)}_Asc")]
public class SessionUnit : BaseSessionEntity<Guid>, IChatOwner<long>, ISorting, ISessionId, IHasSimpleStateCheckers<SessionUnit>, IMaterializationInterceptor, ISessionUnit
{

    public List<ISimpleStateChecker<SessionUnit>> StateCheckers => [];

    /// <summary>
    /// 用户Id
    /// </summary>
    [Comment("用户Id")]
    public virtual Guid? AppUserId { get; protected set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    [Required]
    [Comment("会话Id")]
    public virtual Guid? SessionId { get; protected set; }

    /// <summary>
    /// 会话
    /// </summary>
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; protected set; }

    /// <summary>
    /// Key
    /// </summary>
    [MaxLength(450)]
    public virtual string Key { get; protected set; }

    /// <summary>
    /// 目标对象类型
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationObjectType { get; protected set; }

    /// <summary>
    /// 所有者对象类型
    /// </summary>
    public virtual ChatObjectTypeEnums? OwnerObjectType { get; protected set; }

    ///// <summary>
    ///// 已读的消息
    ///// </summary>
    //public virtual long? ReadedMessageId { get; protected set; }

    //[ForeignKey(nameof(ReadedMessageId))]
    //public virtual Message ReadedMessage { get; protected set; }

    /// <summary>
    /// 最后一条消息Id
    /// </summary>
    [Comment("最后一条消息Id")]
    public virtual long? LastMessageId { get; protected set; }

    /// <summary>
    /// 最后一条消息Id
    /// </summary>
    [ForeignKey(nameof(LastMessageId))]
    [Comment("最后一条消息Id")]
    public virtual Message LastMessage { get; protected set; }

    /// <summary>
    /// 消息角标,包含了私有消息 PrivateBadge (未读消息数量)
    /// </summary>
    [Comment("消息角标,包含了私有消息 PrivateBadge (未读消息数量)")]
    public virtual int PublicBadge { get; protected set; }

    /// <summary>
    /// 私有消息角标(未读消息数量)
    /// </summary>
    [Comment("私有消息角标(未读消息数量)")]
    public virtual int PrivateBadge { get; protected set; }

    /// <summary>
    /// 提醒器数量(@所有人)
    /// </summary>
    [Comment("提醒器数量(@所有人)")]
    public virtual int RemindAllCount { get; protected set; }

    /// <summary>
    /// 提醒器数量(@我)
    /// </summary>
    [Comment("提醒器数量(@我)")]
    public virtual int RemindMeCount { get; protected set; }

    /// <summary>
    /// 特别关注消息数量 FollowingMessageCount
    /// </summary>
    [Comment("特别关注数量")]
    public virtual int FollowingCount { get; protected set; }

    /// <summary>
    /// 置顶(排序)
    /// </summary>
    [Comment("置顶(排序)")]
    public virtual double Sorting { get; protected set; }

    /// <summary>
    /// 时间
    /// </summary>
    [Comment("时间")]
    public virtual double Ticks { get; protected set; } = DateTime.Now.Ticks;

    /// <summary>
    /// 计数器（弃用）
    /// </summary>
    [Required]
    [Comment("计数器（弃用）")]
    [InverseProperty(nameof(SessionUnitCounter.SessionUnit))]
    public virtual SessionUnitCounter Counter { get; protected set; } = new SessionUnitCounter();

    [InverseProperty(nameof(SessionUnitSetting.SessionUnit))]
    public virtual SessionUnitSetting Setting { get; protected set; } //= new SessionUnitSetting();

    [InverseProperty(nameof(SessionUnitSetting.Inviter))]
    public virtual List<SessionUnitSetting> InviterList { get; protected set; }

    [InverseProperty(nameof(SessionUnitSetting.Killer))]
    public virtual List<SessionUnitSetting> KillerList { get; protected set; }

    public virtual IList<MessageReminder> ReminderList { get; protected set; } = [];

    public virtual List<SessionUnitTag> SessionUnitTagList { get; protected set; } = [];

    public virtual List<SessionUnitRole> SessionUnitRoleList { get; protected set; } = [];

    public virtual List<SessionUnitOrganization> SessionUnitOrganizationList { get; protected set; }

    public virtual IList<SessionPermissionUnitGrant> GrantList { get; set; }

    [InverseProperty(nameof(SessionRequest.Handler))]
    public virtual IList<SessionRequest> HandlerList { get; protected set; }

    /// <summary>
    /// sender message list
    /// </summary>
    [InverseProperty(nameof(Message.SenderSessionUnit))]
    public virtual List<Message> MessageList { get; protected set; } = [];

    [InverseProperty(nameof(OpenedRecorder.SessionUnit))]
    public virtual IList<OpenedRecorder> OpenedRecorderList { get; protected set; }

    [InverseProperty(nameof(ReadedRecorder.SessionUnit))]
    public virtual IList<ReadedRecorder> ReadedRecorderList { get; protected set; }

    [InverseProperty(nameof(DeletedRecorder.SessionUnit))]
    public virtual IList<DeletedRecorder> DeletedRecorderList { get; protected set; }

    /// <summary>
    /// 我关注的人（我发起的关注）
    /// </summary>
    [InverseProperty(nameof(Follow.OwnerSessionUnit))]
    public virtual IList<Follow> FollowingList { get; protected set; }

    /// <summary>
    /// 关注我的人（我是被关注者）
    /// </summary>
    [InverseProperty(nameof(Follow.DestinationSessionUnit))]
    public virtual IList<Follow> FollowerList { get; protected set; }

    /// <summary>
    /// 关注的消息列表
    /// </summary>
    [InverseProperty(nameof(MessageFollower.SessionUnit))]
    public virtual IList<MessageFollower> MessageFollowerList { get; protected set; } = [];


    [InverseProperty(nameof(FavoritedRecorder.SessionUnit))]
    public virtual IList<FavoritedRecorder> FavoriteList { get; protected set; }

    [InverseProperty(nameof(SessionUnitEntryValue.SessionUnit))]
    public virtual IList<SessionUnitEntryValue> Entries { get; set; }

    public virtual IList<SessionUnitContactTag> SessionUnitContactTagList { get; set; }

    [NotMapped]
    public virtual List<ContactTag> ContactTags => SessionUnitContactTagList.Select(x => x.Tag).ToList();

    [NotMapped]
    public virtual List<SessionTag> TagList => GetTagList();

    [NotMapped]
    public virtual List<Guid> TagIdList => GetTagIdList();

    [NotMapped]
    public virtual List<SessionRole> RoleList => GetRoleList();

    [NotMapped]
    public virtual List<Guid> RoleIdList => GetRoleIdList();

    [NotMapped]
    public virtual SessionUnitStatModel Stat { get; protected set; }

    [NotMapped]
    public virtual int? Badge { get; protected set; }

    /// <summary>
    /// 是否客服
    /// </summary>
    [NotMapped]
    public virtual bool IsWaiter => IsWaiterOfDestination() && IsWaiterOfOwner();

    /// <summary>
    /// 备注名称
    /// </summary>
    [NotMapped] 
    public virtual string Rename => Setting.Rename;

    [NotMapped]
    public virtual string DisplayName => Setting.MemberName;

    [NotMapped]
    public virtual string MemberName => Setting.MemberName;

    [NotMapped]
    public virtual bool IsPublic => Setting.IsPublic;

    [NotMapped]
    public virtual bool IsStatic => Setting.IsStatic;

    [NotMapped]
    public virtual bool IsCreator => Setting.IsCreator;

    [NotMapped]
    public virtual bool IsVisible => Setting.IsVisible;

    [NotMapped]
    public virtual bool IsEnabled => Setting.IsEnabled;

    [NotMapped]
    public virtual long? ReadedMessageId => Setting.ReadedMessageId;

    protected SessionUnit() { }

    internal SessionUnit(ISessionUnitIdGenerator idGenerator,
        [NotNull]
        Session session,
        [NotNull]
        ChatObject owner,
        [NotNull]
        ChatObject destination,
        Action<SessionUnitSetting> setting)
    {
        Id = idGenerator.Create(owner.Id, destination.Id);
        SetKey(idGenerator.Generate(owner.Id, destination.Id));
        Session = session;
        SessionId = session.Id;
        Setting = new SessionUnitSetting(Session);

        Owner = owner;
        OwnerId = owner.Id;
        OwnerObjectType = owner.ObjectType;

        Destination = destination;
        DestinationId = destination.Id;
        DestinationObjectType = destination.ObjectType;

        setting?.Invoke(Setting);
    }

    public static Expression<Func<SessionUnit, bool>> GetActivePredicate(DateTime? messageCreationTime = null)
    {
        var creationTime = messageCreationTime ?? DateTime.Now;
        return x =>
            !x.IsDeleted &&
            !x.Setting.IsKilled &&
            x.Setting.IsEnabled &&
            (x.Setting.HistoryFristTime == null || creationTime > x.Setting.HistoryFristTime) &&
            (x.Setting.HistoryLastTime == null || creationTime < x.Setting.HistoryLastTime) &&
            (x.Setting.ClearTime == null || creationTime > x.Setting.ClearTime)
        ;
    }

    internal virtual void UpdateCounter(SessionUnitCounterInfo counter)
    {
        Setting.ReadedMessageId = counter.ReadedMessageId;
        PublicBadge = counter.PublicBadge;
        PrivateBadge = counter.PrivateBadge;
        RemindAllCount = counter.RemindAllCount;
        RemindMeCount = counter.RemindMeCount;
        FollowingCount = counter.FollowingCount;
        //Ticks = DateTime.Now.Ticks;
    }

    public virtual void SetKey(string key)
    {
        Key = key;
    }

    public SessionUnit SetBadge(int badge)
    {
        Badge = badge;
        return this;
    }

    public virtual bool IsWaiterOfDestination()
    {
        var destinationObjectTypes = new List<ChatObjectTypeEnums?>() { ChatObjectTypeEnums.Personal, ChatObjectTypeEnums.Anonymous, ChatObjectTypeEnums.Customer };

        return destinationObjectTypes.Contains(DestinationObjectType);
    }

    public virtual bool IsWaiterOfOwner()
    {
        var shopObjectTypes = new List<ChatObjectTypeEnums?>() { ChatObjectTypeEnums.ShopKeeper, ChatObjectTypeEnums.ShopWaiter };

        return shopObjectTypes.Contains(OwnerObjectType);
    }

    internal virtual void SetTopping(bool isTopping)
    {
        Sorting = isTopping ? DateTime.Now.Ticks : 0;
    }

    internal virtual void SetTicks(double? ticks)
    {
        Ticks = ticks ?? 0;
    }

    private List<SessionTag> GetTagList()
    {
        return SessionUnitTagList.Select(x => x.SessionTag).ToList();
    }

    private List<Guid> GetTagIdList() => SessionUnitTagList.Select(x => x.SessionTagId).ToList();

    private List<SessionRole> GetRoleList() => SessionUnitRoleList.Select(x => x.SessionRole).ToList();

    private List<Guid> GetRoleIdList() => SessionUnitRoleList.Select(x => x.SessionRoleId).ToList();
}
