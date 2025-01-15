using IczpNet.Chat.Commands;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.MessageSections.Messages
{
    public class MessageWithQuoteInfo<T> : MessageInfo<T>
    {
        public virtual MessageInfo QuoteMessage { get; set; }
    }

    public class MessageWithQuoteInfo : MessageInfo
    {
        public virtual MessageInfo QuoteMessage { get; set; }
    }

    public class MessageAnyInfo : MessageInfo<object>
    {

    }

    public class MessageInfo<T> : MessageInfo
    {
        public T Content { get; set; }
    }

    [Command(CommandConsts.Chat)]
    public class MessageInfo
    {
        /// <summary>
        /// 消息Id
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// 发送人
        /// </summary>
        public virtual string SenderName { get; set; }

        /// <summary>
        ///  消息类型
        /// </summary>
        public virtual MessageTypes MessageType { get; set; }

        /// <summary>
        /// 提醒类型
        /// </summary>
        public virtual ReminderTypes? ReminderType { get; set; }

        /// <summary>
        /// 是否私有消息
        /// </summary>
        public virtual bool IsPrivate { get; set; }

        /// <summary>
        /// 是否撤回
        /// </summary>
        public virtual bool IsRollbacked { get; set; }

        /// <summary>
        /// 是否@所有人
        /// </summary>
        public virtual bool IsRemindAll { get; set; }

        /// <summary>
        /// 撤回消息时间
        /// </summary>
        public virtual DateTime? RollbackTime { get; set; }

        /// <summary>
        /// 创建时间（发送时间）
        /// </summary>
        public virtual DateTime CreationTime { get; set; }

        /// <summary>
        /// 发送人信息
        /// </summary>
        public virtual SessionUnitSenderInfo SenderSessionUnit { get; set; }
    }
}
