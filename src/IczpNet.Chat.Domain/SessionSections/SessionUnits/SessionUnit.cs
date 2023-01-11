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
    [Index(nameof(LastMessageAutoId))]
    //[Index(nameof(Sorting), nameof(LastMessageAutoId))]
    [Index(nameof(Sorting))]
    public class SessionUnit : BaseSessionEntity, IChatOwner<Guid>, ISorting
    {
        public virtual Guid SessionId { get; protected set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; protected set; }

        public virtual ChatObjectTypes? DestinationObjectType { get; protected set; }

        [StringLength(50)]
        public virtual string Rename { get; protected set; }

        /// <summary>
        /// 已读的消息AutoId
        /// </summary>
        public virtual long ReadedMessageAutoId { get; protected set; }

        public virtual long LastMessageAutoId { get; protected set; }

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

        public virtual Guid? KillerId { get; protected set; }

        [ForeignKey(nameof(KillerId))]
        public virtual ChatObject Killer { get; protected set; }

        public virtual DateTime? ClearTime { get; protected set; }

        /// <summary>
        /// 不显示消息会话(不退群,不删除消息)
        /// </summary>
        public virtual DateTime? RemoveTime { get; protected set; }

        /// <summary>
        /// 消息免打扰，默认为 false
        /// </summary>
        public virtual bool IsImmersed { get; protected set; }

        /// <summary>
        /// 加入方式
        /// </summary>
        public virtual JoinWays? JoinWay { get; set; }

        /// <summary>
        /// 邀请人
        /// </summary>
        public virtual Guid? InviterId { get; set; }

        [ForeignKey(nameof(InviterId))]
        public virtual ChatObject Inviter { get; set; }

        public virtual double Sorting { get; protected set; }

        public virtual IList<MessageReminder> ReminderList { get; protected set; } = new List<MessageReminder>();

        public virtual List<SessionUnitTag> SessionUnitTagList { get; protected set; } = new List<SessionUnitTag>();

        public virtual List<SessionUnitRole> SessionUnitRoleList { get; protected set; } = new List<SessionUnitRole>();

        [NotMapped]
        public virtual int ReminderAllCount => GetRemindAllCount();

        [NotMapped]
        public virtual int ReminderMeCount => GetRemindMeCount();

        [NotMapped]
        public virtual int Badge => Session.MessageList.Count(x =>
                //!x.IsRollbacked &&
                x.AutoId > ReadedMessageAutoId &&
                x.SenderId != OwnerId &&
                (!HistoryFristTime.HasValue || x.CreationTime > HistoryFristTime) &&
                (!HistoryLastTime.HasValue || x.CreationTime < HistoryLastTime) &&
                (!ClearTime.HasValue || x.CreationTime > ClearTime));// GetBadge();

        [NotMapped]
        public virtual int ReminderCount => GetReminderCount();

        [NotMapped]
        public virtual Message LastMessage => Session.LastMessage;//GetLastMessage();

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
        public virtual long? SessionLastMessageAutoId => Session.LastMessageAutoId;

        protected SessionUnit() { }

        internal SessionUnit(Guid id, [NotNull] Session session, [NotNull] ChatObject owner, [NotNull] ChatObject destination) : base(id)
        {
            Session = session;
            Owner = owner;
            Destination = destination;
            DestinationObjectType = destination.ObjectType;
        }

        internal void SetReaded(long messageAutoId, bool isForce = false)
        {
            if (isForce || messageAutoId > ReadedMessageAutoId)
            {
                ReadedMessageAutoId = messageAutoId;
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
        internal void RemoveSession(DateTime removeTime)
        {
            RemoveTime = removeTime;
        }

        /// <summary>
        /// 退群，但不删除会话（用于查看历史I）
        /// </summary>
        /// <param name="removeTime"></param>
        internal void KillSession(DateTime killTime, KillTypes? killType = null, ChatObject killer = null)
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
            return Session.MessageList.AsQueryable().OrderByDescending(x => x.AutoId).FirstOrDefault(new SessionUnitMessageSpecification(this).ToExpression());
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
    }
}
