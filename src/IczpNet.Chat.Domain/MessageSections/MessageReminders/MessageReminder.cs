using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.MessageReminders;

/// <summary>
/// 消息提醒器@我
/// </summary>
[Description("消息提醒器@我")]
public class MessageReminder : BaseEntity
{
    /// <summary>
    /// 
    /// </summary>
    public virtual Guid SessionUnitId { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long MessageId { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [ForeignKey(nameof(MessageId))]
    public virtual Message Message { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ReminderTypes ReminderType { get; protected set; }

    protected MessageReminder() { }

    internal MessageReminder(Message message, SessionUnit sessionUnit, ReminderTypes reminderType)
    {
        Message = message;
        SessionUnit = sessionUnit;
        ReminderType = reminderType;
    }

    internal MessageReminder(Guid sessionUnitId, ReminderTypes reminderType)
    {
        SessionUnitId = sessionUnitId;
        ReminderType = reminderType;
    }

    public override object[] GetKeys()
    {
        return [SessionUnitId, MessageId];
    }
}
