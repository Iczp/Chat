using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.MessageReminders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    /// <summary>
    /// @所有人 Remind Everyone 
    /// </summary>
    public virtual bool IsRemindAll { get; protected set; }

    /// <summary>
    /// 是否@我
    /// </summary>
    [NotMapped]
    public virtual bool? IsRemindMe { get; set; }

    /// <summary>
    /// 提醒器类型
    /// </summary>
    public virtual ReminderTypes? ReminderType { get; protected set; }

    [InverseProperty(nameof(MessageReminder.Message))]
    public virtual IList<MessageReminder> MessageReminderList { get; protected set; }

    /// <summary>
    /// 设置提醒所有人 @Everyone
    /// </summary>
    public virtual void SetRemindAll()
    {
        SetKey(MessageKeyNames.Remind, MessageKeyNames.RemindEveryone);
        IsRemindAll = true;
    }

    /// <summary>
    /// 设置提醒指定人
    /// </summary>
    public virtual void SetReminder(List<Guid> sessionUnitIdList, ReminderTypes reminderType)
    {
        SetKey($"{MessageKeyNames.Remind}:{reminderType}", sessionUnitIdList.JoinAsString(","));
        MessageReminderList = sessionUnitIdList.Select(x => new MessageReminder(x, reminderType)).ToList();
    }
}
