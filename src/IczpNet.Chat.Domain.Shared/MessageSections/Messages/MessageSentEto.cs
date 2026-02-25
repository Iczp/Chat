using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages;

public class MessageSentEto
{
    public long Id { get; set; }

    public List<Guid> ReminderIdList { get; set; } = [];

    public List<Guid> FollowerIdList { get; set; } = [];

    public string HostName { get; set; }

    public DateTime? PublishTime {  get; set; }

    public override string ToString()
    {
        return $"{nameof(MessageSentEto)}:{nameof(Id)}={Id},{nameof(HostName)}={HostName},{nameof(PublishTime)}={PublishTime}";
    }
}
