using IczpNet.AbpCommons.DataFilters;
using IczpNet.AbpCommons.PinYin;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseEntitys;
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
using IczpNet.Chat.Specifications;
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

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    [Index(nameof(Sorting), AllDescending = true)]
    [Index(nameof(LastMessageId), IsDescending = new[] { true })]
    [Index(nameof(Sorting), nameof(LastMessageId), AllDescending = true)]
    [Index(nameof(ReadedMessageId), AllDescending = true)]
    [Index(nameof(OwnerId), nameof(DestinationId), AllDescending = true)]
    [Index(nameof(IsStatic), AllDescending = true)]
    [Index(nameof(IsPublic), AllDescending = true)]
    [Index(nameof(Key), AllDescending = true)]
    [Index(nameof(DestinationObjectType), AllDescending = true)]
    [Index(nameof(DestinationObjectType), AllDescending = false)]

    [Index(nameof(OwnerName), AllDescending = true)]
    [Index(nameof(OwnerNameSpellingAbbreviation), AllDescending = true)]

    [Index(nameof(DestinationName), AllDescending = true)]
    [Index(nameof(DestinationNameSpellingAbbreviation), AllDescending = true)]

    [Index(nameof(MemberName), AllDescending = true)]
    [Index(nameof(MemberNameSpellingAbbreviation), AllDescending = true)]

    [Index(nameof(Rename), AllDescending = true)]
    [Index(nameof(RenameSpellingAbbreviation), AllDescending = true)]
    public class SessionUnit : BaseSessionEntity<Guid>, IChatOwner<long>, ISorting, IIsStatic, IIsPublic, ISessionId, IHasSimpleStateCheckers<SessionUnit>, IMaterializationInterceptor
    {

        public List<ISimpleStateChecker<SessionUnit>> StateCheckers => new();

        public virtual Guid? AppUserId { get; protected set; }

        [Required]
        public virtual Guid? SessionId { get; protected set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; protected set; }

        [MaxLength(450)]
        public virtual string Key { get; protected set; }

        public virtual ChatObjectTypeEnums? DestinationObjectType { get; protected set; }

        public virtual ChatObjectTypeEnums? OwnerObjectType { get; protected set; }

        /// <summary>
        /// 已读的消息
        /// </summary>
        public virtual long? ReadedMessageId { get; protected set; }

        [ForeignKey(nameof(ReadedMessageId))]
        public virtual Message ReadedMessage { get; protected set; }

        public virtual long? LastMessageId { get; protected set; }

        [ForeignKey(nameof(LastMessageId))]
        public virtual Message LastMessage { get; protected set; }

        /// <summary>
        /// 为null时，
        /// </summary>
        public virtual DateTime? HistoryFristTime { get; protected set; }

        public virtual DateTime? HistoryLastTime { get; protected set; }

        /// <summary>
        /// KillSession  退出群，但是不删除会话(用于查看历史消息)
        /// </summary>
        public virtual bool IsKilled { get; protected set; }

        public virtual KillTypes? KillType { get; protected set; }

        public virtual DateTime? KillTime { get; protected set; }

        public virtual long? KillerId { get; protected set; }

        [ForeignKey(nameof(KillerId))]
        public virtual ChatObject Killer { get; protected set; }

        public virtual DateTime? ClearTime { get; protected set; }

        /// <summary>
        /// 不显示消息会话(不退群,不删除消息)
        /// </summary>
        public virtual DateTime? RemoveTime { get; protected set; }

        /// <summary>
        /// 会话内的名称
        /// </summary>
        [MaxLength(50)]
        public virtual string MemberName { get; protected set; }

        [MaxLength(300)]
        public virtual string MemberNameSpelling { get; protected set; }

        [MaxLength(50)]
        public virtual string MemberNameSpellingAbbreviation { get; protected set; }

        /// <summary>
        /// 备注名称 Rename for destination
        /// </summary>
        [MaxLength(50)]
        public virtual string Rename { get; protected set; }

        [MaxLength(300)]
        public virtual string RenameSpelling { get; protected set; }

        [MaxLength(50)]
        public virtual string RenameSpellingAbbreviation { get; protected set; }

        [MaxLength(50)]
        public virtual string OwnerName { get; protected set; }

        [MaxLength(50)]
        public virtual string OwnerNameSpellingAbbreviation { get; protected set; }

        [MaxLength(50)]
        public virtual string DestinationName { get; protected set; }

        [MaxLength(50)]
        public virtual string DestinationNameSpellingAbbreviation { get; protected set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(500)]
        public virtual string Remarks { get; protected set; }

        /// <summary>
        /// 是否保存通讯录(群)
        /// </summary>
        public virtual bool IsCantacts { get; protected set; }

        /// <summary>
        /// 消息免打扰，默认为 false
        /// </summary>
        public virtual bool IsImmersed { get; protected set; }

        /// <summary>
        /// 是否显示成员名称
        /// </summary>
        public virtual bool IsShowMemberName { get; protected set; }

        /// <summary>
        /// 是否显示已读
        /// </summary>
        public virtual bool IsShowReaded { get; protected set; }

        /// <summary>
        /// 特别关注 for destination
        /// </summary>
        public virtual bool IsImportant { get; protected set; }

        /// <summary>
        /// 聊天背景，默认为 null
        /// </summary>
        [StringLength(500)]
        public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 加入方式
        /// </summary>
        public virtual JoinWays? JoinWay { get; set; }

        /// <summary>
        /// 邀请人
        /// </summary>
        public virtual long? InviterId { get; set; }

        #region 邀请人 SessionUnit
        public virtual Guid? InviterUnitId { get; set; }

        [ForeignKey(nameof(InviterUnitId))]
        public virtual SessionUnit InviterUnit { get; set; }

        [InverseProperty(nameof(InviterUnit))]
        public virtual List<SessionUnit> InviterUnitList { get; set; }
        #endregion

        #region 删除人 SessionUnit
        public virtual Guid? KillerUnitId { get; set; }

        [ForeignKey(nameof(KillerUnitId))]
        public virtual SessionUnit KillerUnit { get; set; }

        [InverseProperty(nameof(KillerUnit))]
        public virtual List<SessionUnit> KillerUnitList { get; set; }
        #endregion

        /// <summary>
        /// 客服状态
        /// </summary>
        public virtual ServiceStatus ServiceStatus { get; protected set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual bool IsInputEnabled { get; protected set; } = true;

        public virtual bool IsEnabled { get; protected set; } = true;

        /// <summary>
        /// 是否创建者（群主等）
        /// </summary>
        public virtual bool IsCreator { get; protected set; } = false;

        [ForeignKey(nameof(InviterId))]
        public virtual ChatObject Inviter { get; set; }

        public virtual double Sorting { get; protected set; }

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
        [InverseProperty(nameof(Message.SessionUnit))]
        public virtual List<Message> MessageList { get; protected set; } = new List<Message>();

        [InverseProperty(nameof(Follow.Owner))]
        public virtual IList<Follow> OwnerFollowList { get; protected set; }

        [InverseProperty(nameof(OpenedRecorder.SessionUnit))]
        public virtual IList<OpenedRecorder> OpenedRecorderList { get; protected set; }

        //[InverseProperty(nameof(Follow.Destination))]
        //public virtual IList<Follow> DestinationFollowList { get; set; }

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
        public virtual int? Badge { get; protected set; }//=> GetBadge();

        [NotMapped]
        public virtual int? ReminderCount { get; protected set; }//=> GetReminderCount();

        public virtual int PublicBadge { get; protected set; }

        public virtual int PrivateBadge { get; protected set; }

        //[NotMapped]
        public virtual int RemindAllCount { get; protected set; }//=> GetRemindAllCount();

        //[NotMapped]
        public virtual int RemindMeCount { get; protected set; }//=> GetRemindMeCount();

        //[NotMapped]
        public virtual int FollowingCount { get; protected set; }//=> GetFollowingCount();

        protected SessionUnit() { }

        internal SessionUnit(ISessionUnitIdGenerator idGenerator,
            [NotNull]
            Session session,
            [NotNull]
            long ownerId,
            [NotNull]
            long destinationId,
            ChatObjectTypeEnums? destinationObjectType,
            bool isPublic = true,
            bool isStatic = false,
            bool isCreator = false,
            JoinWays? joinWay = null,
            Guid? inviterUnitId = null,
            bool isInputEnabled = true)
        {
            Id = idGenerator.Create(ownerId, destinationId);
            SetKey(idGenerator.Generate(ownerId, destinationId));
            Session = session;
            OwnerId = ownerId;
            DestinationId = destinationId;
            DestinationObjectType = destinationObjectType;
            IsStatic = isStatic;
            IsPublic = isPublic;
            IsCreator = isCreator;
            JoinWay = joinWay;
            InviterUnitId = inviterUnitId;
            IsInputEnabled = isInputEnabled;
        }
        internal SessionUnit(ISessionUnitIdGenerator idGenerator,
            [NotNull]
            Session session,
            [NotNull]
            ChatObject owner,
            [NotNull]
            ChatObject destination,
            bool isPublic = true,
            bool isStatic = false,
            bool isCreator = false,
            JoinWays? joinWay = null,
            Guid? inviterUnitId = null,
            bool isInputEnabled = true)
        {
            Id = idGenerator.Create(owner.Id, destination.Id);
            SetKey(idGenerator.Generate(owner.Id, destination.Id));
            Session = session;
            SessionId = session.Id;

            Owner = owner;
            OwnerId = owner.Id;
            OwnerName = owner.Name;
            OwnerNameSpellingAbbreviation = owner.NameSpellingAbbreviation;
            OwnerObjectType = owner.ObjectType;

            Destination = destination;
            DestinationId = destination.Id;
            DestinationName =destination.Name;
            DestinationNameSpellingAbbreviation = destination.NameSpellingAbbreviation;
            DestinationObjectType = destination.ObjectType;

            IsStatic = isStatic;
            IsPublic = isPublic;
            IsCreator = isCreator;
            JoinWay = joinWay;
            InviterUnitId = inviterUnitId;
            IsInputEnabled = isInputEnabled;
        }

        public static Expression<Func<SessionUnit, bool>> GetActivePredicate(DateTime? messageCreationTime)
        {
            var creationTime = messageCreationTime ?? DateTime.Now;
            return x =>
                !x.IsDeleted &&
                !x.IsKilled &&
                x.IsEnabled &&
                x.ServiceStatus == ServiceStatus.Normal &&
                (x.HistoryFristTime == null || creationTime > x.HistoryFristTime) &&
                (x.HistoryLastTime == null || creationTime < x.HistoryLastTime) &&
                (x.HistoryLastTime == null || creationTime < x.HistoryLastTime) &&
                (x.ClearTime == null || creationTime > x.ClearTime)
            ;
        }

        public virtual void SetKey(string key)
        {
            Key = key;
        }

        internal virtual void SetRename(string rename)
        {
            Rename = rename;
            RenameSpelling = rename.ConvertToPinyin().MaxLength(300);
            RenameSpellingAbbreviation = rename.ConvertToPY().MaxLength(50);
        }

        internal virtual void SetMemberName(string memberName)
        {
            MemberName = memberName;
            MemberNameSpelling = memberName.ConvertToPinyin().MaxLength(300);
            MemberNameSpellingAbbreviation = memberName.ConvertToPY().MaxLength(50);
        }

        internal virtual void SetReaded(long lastMessageId, bool isForce = false)
        {
            if (isForce || lastMessageId > ReadedMessageId.GetValueOrDefault())
            {
                ReadedMessageId = lastMessageId;
            }
            PublicBadge = 0;
            PrivateBadge = 0;
            FollowingCount = 0;
            RemindAllCount = 0;
            RemindMeCount = 0;
        }

        internal virtual void SetHistoryFristTime(DateTime historyFristTime)
        {
            HistoryFristTime = historyFristTime;
        }

        /// <summary>
        /// removeSession 删除消息会话,不退群
        /// </summary>
        /// <param name="removeTime"></param>
        internal virtual void Remove(DateTime removeTime)
        {
            RemoveTime = removeTime;
        }

        /// <summary>
        /// 退群，但不删除会话（用于查看历史I）
        /// </summary>
        /// <param name="removeTime"></param>
        internal virtual void Kill(DateTime killTime, KillTypes? killType = null, ChatObject killer = null)
        {
            IsKilled = true;
            KillTime = killTime;
            HistoryLastTime = killTime;
            KillType = killType;
            if (killer != null)
            {
                Killer = killer;
            }
        }

        /// <summary>
        /// 清空消息，不退群 
        /// </summary>
        /// <param name="clearTime"></param>
        internal virtual void ClearMessage(DateTime? clearTime)
        {
            ClearTime = clearTime;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionId, OwnerId, DestinationId };
        }

        public SessionUnit SetBadge(int badge)
        {
            Badge = badge;
            return this;
        }

        public void SetReminderCount(int value)
        {
            RemindAllCount = value;
        }

        public void SetFollowingCount(int value)
        {
            FollowingCount = value;
        }

        protected virtual int GetBadge()
        {
            return Session.MessageList.AsQueryable().Count(new SessionUnitMessageSpecification(this).ToExpression());
        }

        protected virtual Message GetLastMessage()
        {
            return Session.MessageList.AsQueryable().OrderByDescending(x => x.Id).FirstOrDefault(new SessionUnitMessageSpecification(this).ToExpression());
        }

        private int GetReminderCount()
        {
            return GetRemindMeCount() + GetRemindAllCount();
        }

        /// <summary>
        /// @me
        /// </summary>
        /// <returns></returns>
        protected virtual int GetRemindMeCount()
        {
            return ReminderList.AsQueryable().Select(x => x.Message).Where(x => !x.IsRollbacked).Count(new SessionUnitMessageSpecification(this).ToExpression());
        }

        /// <summary>
        /// @everyone
        /// </summary>
        /// <returns></returns>
        protected virtual int GetRemindAllCount()
        {
            return Session.MessageList.AsQueryable().Where(x => x.IsRemindAll && !x.IsRollbacked).Count(new SessionUnitMessageSpecification(this).ToExpression());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual int GetFollowingCount()
        {
            return Session.MessageList.AsQueryable().Where(x => OwnerFollowList.Any(d => d.DestinationId == x.SessionUnitId)).Count(new SessionUnitMessageSpecification(this).ToExpression());
        }

        internal virtual void SetTopping(bool isTopping)
        {
            Sorting = isTopping ? DateTime.Now.Ticks : 0;
        }

        private List<SessionTag> GetTagList()
        {
            return SessionUnitTagList.Select(x => x.SessionTag).ToList();
        }

        private List<Guid> GetTagIdList()
        {
            return SessionUnitTagList.Select(x => x.SessionTagId).ToList();
        }

        private List<SessionRole> GetRoleList()
        {
            return SessionUnitRoleList.Select(x => x.SessionRole).ToList();
        }

        private List<Guid> GetRoleIdList()
        {
            return SessionUnitRoleList.Select(x => x.SessionRoleId).ToList();
        }

        internal virtual void SetImmersed(bool isImmersed)
        {
            IsImmersed = isImmersed;
        }

        internal virtual void SetIsEnabled(bool v)
        {
            IsEnabled = v;
        }

        internal virtual void SetIsCreator(bool v)
        {
            IsCreator = v;
            IsStatic = v;
        }

        internal void SetLastMessage(Message message)
        {
            LastMessageId = message.Id;
            LastMessage = message;
        }

        internal void SetPrivateBadge(int v)
        {
            PrivateBadge = v;
        }
    }
}
