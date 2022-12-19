using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.MessageReminders;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionUnit : BaseSessionEntity, IChatOwner<Guid>
    {
        public virtual Guid SessionId { get; protected set; }

        public virtual int Badge => GetBadge();

        public virtual int ReminderCount => GetReminderCount();

        public virtual Message LastMessage => GetLastMessage();

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; protected set; }

        //public virtual Guid OwnerId { get; protected set; }

        //[ForeignKey(nameof(OwnerId))]
        //public virtual ChatObject Owner { get; protected set; }

        //public virtual Guid? DestinationId { get; protected set; }

        //[ForeignKey(nameof(DestinationId))]
        //public virtual ChatObject Destination { get; protected set; }

        public virtual long ReadedMessageAutoId { get; protected set; }


        /// <summary>
        /// 为null时，
        /// </summary>
        public virtual DateTime? HistoryFristTime { get; protected set; }

        public virtual DateTime? HistoryLastTime { get; protected set; }

        /// <summary>
        /// KillSession  退出群，不删除会话
        /// </summary>
        public virtual bool IsKilled { get; protected set; }

        public virtual DateTime? ClearTime { get; protected set; }

        /// <summary>
        /// 删除消息会话,不退群
        /// </summary>
        public virtual DateTime? RemoveTime { get; protected set; }

        public virtual string Name { get; set; }

        public virtual IList<MessageReminder> ReminderList { get; protected set; }

        protected SessionUnit() { }

        internal SessionUnit(Guid id, [NotNull] Guid sessionId, [NotNull] Guid ownerId, [NotNull] Guid destinationId) : base(id)
        {
            SessionId = sessionId;
            OwnerId = ownerId;
            DestinationId = destinationId;
        }

        internal void SetReaded(long messageAutoId)
        {
            ReadedMessageAutoId = messageAutoId;
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
        internal void KillSession(DateTime killTime)
        {
            IsKilled = true;
            //KillTime = killTime;
            HistoryLastTime = killTime;
        }

        /// <summary>
        /// 清空消息，不退群 
        /// </summary>
        /// <param name="clearTime"></param>
        internal void ClearMessage(DateTime clearTime)
        {
            ClearTime = clearTime;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionId, OwnerId, DestinationId };
        }

        protected virtual int GetBadge()
        {
            return Session.MessageList.AsQueryable().Count(x => !x.IsRollbacked && x.AutoId > ReadedMessageAutoId && x.SenderId != OwnerId && (!HistoryFristTime.HasValue || x.CreationTime > HistoryFristTime));
        }

        protected virtual Message GetLastMessage()
        {
            return Session.MessageList.FirstOrDefault(x => x.AutoId == Session.MessageList.Max(d => d.AutoId));
        }

        protected virtual int GetReminderCount()
        {
            return ReminderList.Count + Session.MessageList.Count(x => x.IsRemindAll && !x.IsRollbacked && x.AutoId > ReadedMessageAutoId && x.SenderId != OwnerId && (!HistoryFristTime.HasValue || x.CreationTime > HistoryFristTime));
            //x => x.Message.IsRemindAll || x.MessageReminderList.Any(d => d.SessionUnitId == SessionUnitId)
        }

    }
}
