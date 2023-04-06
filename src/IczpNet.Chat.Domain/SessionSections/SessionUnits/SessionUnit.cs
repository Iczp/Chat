using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.MessageReminders;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionTags;
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

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    [Index(nameof(Sorting), AllDescending = true)]
    [Index(nameof(LastMessageId), IsDescending = new[] { true })]
    [Index(nameof(Sorting), nameof(LastMessageId), AllDescending = true)]
    [Index(nameof(ReadedMessageId), AllDescending = true)]
    public class SessionUnit : BaseSessionEntity, IChatOwner<long>, ISorting, IIsStatic, IIsPublic
    {
        public virtual Guid SessionId { get; protected set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; protected set; }

        public virtual ChatObjectTypeEnums? DestinationObjectType { get; protected set; }

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
        [StringLength(50)]
        public virtual string MemberName { get; protected set; }

        /// <summary>
        /// 备注名称 Rename for destination
        /// </summary>
        [StringLength(50)]
        public virtual string Rename { get; protected set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(500)]
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

        /// <summary>
        /// 客服状态
        /// </summary>
        public virtual ServiceStatus ServiceStatus { get; protected set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsPublic { get; set; }

        [ForeignKey(nameof(InviterId))]
        public virtual ChatObject Inviter { get; set; }

        public virtual double Sorting { get; protected set; }

        public virtual IList<MessageReminder> ReminderList { get; protected set; } = new List<MessageReminder>();

        public virtual List<SessionUnitTag> SessionUnitTagList { get; protected set; } = new List<SessionUnitTag>();

        public virtual List<SessionUnitRole> SessionUnitRoleList { get; protected set; } = new List<SessionUnitRole>();

        /// <summary>
        /// sender message list
        /// </summary>
        [InverseProperty(nameof(Message.SessionUnit))]
        public virtual List<Message> MessageList { get; protected set; } = new List<Message>();

        [NotMapped]
        public virtual int ReminderAllCount => GetRemindAllCount();

        [NotMapped]
        public virtual int ReminderMeCount => GetRemindMeCount();

        //[NotMapped]
        //public virtual int Badge => Session.MessageList.Count(x =>
        //        //!x.IsRollbacked &&
        //        (ReadedMessageId == null || x.Id > ReadedMessageId) &&
        //        x.SenderId != OwnerId &&
        //        (!HistoryFristTime.HasValue || x.CreationTime > HistoryFristTime) &&
        //        (!HistoryLastTime.HasValue || x.CreationTime < HistoryLastTime) &&
        //        (!ClearTime.HasValue || x.CreationTime > ClearTime));

        [NotMapped]
        public virtual int Badge => GetBadge();

        [NotMapped]
        public virtual int ReminderCount => GetReminderCount();

        [NotMapped]
        public virtual Message SessionLastMessage => Session.LastMessage;//GetLastMessage();

        [NotMapped]
        public virtual List<SessionTag> TagList => GetTagList();

        [NotMapped]
        public virtual List<Guid> TagIdList => GetTagIdList();

        [NotMapped]
        public virtual List<SessionRole> RoleList => GetRoleList();

        [NotMapped]
        public virtual List<Guid> RoleIdList => GetRoleIdList();

        /// <summary>
        /// last message autoId
        /// </summary>
        [NotMapped]
        public virtual long? SessionLastMessageId => Session.LastMessageId;

        protected SessionUnit() { }

        internal SessionUnit(Guid id, [NotNull] Session session, [NotNull] long ownerId, [NotNull] long destinationId, ChatObjectTypeEnums? destinationObjectType) : base(id)
        {
            Session = session;
            OwnerId = ownerId;
            DestinationId = destinationId;
            DestinationObjectType = destinationObjectType;
        }

        internal SessionUnit(Guid id, [NotNull] Session session, [NotNull] long ownerId, [NotNull] ChatObject destination) : base(id)
        {
            Session = session;
            OwnerId = ownerId;
            Destination = destination;
            DestinationObjectType = destination.ObjectType;
        }

        internal void SetReaded(long messageAutoId, bool isForce = false)
        {
            if (isForce || messageAutoId > ReadedMessageId.GetValueOrDefault())
            {
                ReadedMessageId = messageAutoId;
            }
        }

        internal void SetHistoryFristTime(DateTime historyFristTime)
        {
            HistoryFristTime = historyFristTime;
        }

        /// <summary>
        /// removeSession 删除消息会话,不退群
        /// </summary>
        /// <param name="removeTime"></param>
        internal void Remove(DateTime removeTime)
        {
            RemoveTime = removeTime;
        }

        /// <summary>
        /// 退群，但不删除会话（用于查看历史I）
        /// </summary>
        /// <param name="removeTime"></param>
        internal void Kill(DateTime killTime, KillTypes? killType = null, ChatObject killer = null)
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
        internal void ClearMessage(DateTime? clearTime)
        {
            ClearTime = clearTime;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionId, OwnerId, DestinationId };
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
        protected int GetRemindMeCount()
        {
            return ReminderList.AsQueryable().Select(x => x.Message).Where(x => !x.IsRollbacked).Count(new SessionUnitMessageSpecification(this).ToExpression());
        }

        /// <summary>
        /// @everyone
        /// </summary>
        /// <returns></returns>
        protected int GetRemindAllCount()
        {
            return Session.MessageList.AsQueryable().Where(x => x.IsRemindAll && !x.IsRollbacked).Count(new SessionUnitMessageSpecification(this).ToExpression());
        }

        internal void SetTopping(bool isTopping)
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

        internal void SetImmersed(bool isImmersed)
        {
            IsImmersed = isImmersed;
        }
    }
}
