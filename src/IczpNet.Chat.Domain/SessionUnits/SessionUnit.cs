using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionRequests;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionSections.SessionUnitOrganizations;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using IczpNet.Chat.MessageSections.MessageReminders;
using Volo.Abp.SimpleStateChecking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using IczpNet.Chat.Follows;
using IczpNet.Chat.OpenedRecorders;
using System.Linq.Expressions;
using IczpNet.Chat.FavoritedRecorders;
using IczpNet.Chat.ReadedRecorders;
using IczpNet.Chat.SessionSections.SessionUnitCounters;
using IczpNet.Chat.SessionSections.SessionUnitEntryValues;
using IczpNet.Chat.SessionSections.SessionUnitContactTags;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.SessionSections;
using IczpNet.Chat.SessionSections.SessionUnits;

namespace IczpNet.Chat.SessionUnits
{
    [Index(nameof(Sorting), AllDescending = true)]
    [Index(nameof(Ticks), AllDescending = true)]
    [Index(nameof(LastMessageId), IsDescending = new[] { true })]
    [Index(nameof(Sorting), nameof(Ticks), AllDescending = true)]

    [Index(nameof(Key), AllDescending = true)]
    [Index(nameof(DestinationObjectType), AllDescending = true)]
    [Index(nameof(OwnerObjectType), AllDescending = true)]
    [Index(nameof(IsDeleted))]
    [Index(nameof(CreationTime), AllDescending = true)]

    [Index(nameof(Sorting), nameof(LastMessageId), nameof(IsDeleted), AllDescending = true)]
    [Index(nameof(Sorting), nameof(LastMessageId), nameof(IsDeleted), IsDescending = new[] { true, false, true }, Name = "IX_Chat_SessionUnit_Sorting_Desc_LastMessageId_Asc")]
    public class SessionUnit : BaseSessionEntity<Guid>, IChatOwner<long>, ISorting, ISessionId, IHasSimpleStateCheckers<SessionUnit>, IMaterializationInterceptor
    {

        public List<ISimpleStateChecker<SessionUnit>> StateCheckers => new();

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

        /// <summary>
        /// 备注名称
        /// </summary>
        public virtual string Rename => Setting.Rename;

        public virtual string DisplayName => Setting.MemberName;

        public virtual string MemberName => Setting.MemberName;

        public virtual bool IsPublic => Setting.IsPublic;

        public virtual bool IsStatic => Setting.IsStatic;

        public virtual bool IsCreator => Setting.IsCreator;

        public virtual long? ReadedMessageId => Setting.ReadedMessageId;

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
        /// 特别关注数量
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

        public virtual IList<MessageReminder> ReminderList { get; protected set; } = new List<MessageReminder>();

        public virtual List<SessionUnitTag> SessionUnitTagList { get; protected set; } = new List<SessionUnitTag>();

        public virtual List<SessionUnitRole> SessionUnitRoleList { get; protected set; } = new List<SessionUnitRole>();

        public virtual List<SessionUnitOrganization> SessionUnitOrganizationList { get; protected set; }

        public virtual IList<SessionPermissionUnitGrant> GrantList { get; set; }

        [InverseProperty(nameof(SessionRequest.Handler))]
        public virtual IList<SessionRequest> HandlerList { get; protected set; }

        /// <summary>
        /// sender message list
        /// </summary>
        [InverseProperty(nameof(Message.SenderSessionUnit))]
        public virtual List<Message> MessageList { get; protected set; } = new List<Message>();

        [InverseProperty(nameof(OpenedRecorder.SessionUnit))]
        public virtual IList<OpenedRecorder> OpenedRecorderList { get; protected set; }

        [InverseProperty(nameof(ReadedRecorder.SessionUnit))]
        public virtual IList<ReadedRecorder> ReadedRecorderList { get; protected set; }

        [InverseProperty(nameof(Follow.Owner))]
        public virtual IList<Follow> FollowList { get; protected set; }

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


        protected SessionUnit() { }

        internal SessionUnit(ISessionUnitIdGenerator idGenerator,
            [NotNull]
            Session session,
            [NotNull]
            ChatObject owner,
            [NotNull]
            ChatObject destination,
            bool isPublic = true,
            bool isStatic = false,
            bool isVisible = true,
            bool isDisplay = true,
            bool isCreator = false,
            JoinWays? joinWay = null,
            Guid? inviterUnitId = null,
            bool isInputEnabled = true)
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

            Setting.IsStatic = isStatic;
            Setting.IsPublic = isPublic;
            Setting.IsDisplay = isDisplay;
            Setting.IsVisible = isVisible;
            Setting.SetIsCreator(isCreator);
            Setting.JoinWay = joinWay;
            Setting.InviterId = inviterUnitId;
            Setting.IsInputEnabled = isInputEnabled;
        }

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

        internal virtual void SetTopping(bool isTopping)
        {
            Sorting = isTopping ? DateTime.Now.Ticks : 0;
        }

        private List<SessionTag> GetTagList()
        {
            return SessionUnitTagList.Select(x => x.SessionTag).ToList();
        }

        private List<Guid> GetTagIdList() => SessionUnitTagList.Select(x => x.SessionTagId).ToList();

        private List<SessionRole> GetRoleList() => SessionUnitRoleList.Select(x => x.SessionRole).ToList();

        private List<Guid> GetRoleIdList() => SessionUnitRoleList.Select(x => x.SessionRoleId).ToList();
    }
}
