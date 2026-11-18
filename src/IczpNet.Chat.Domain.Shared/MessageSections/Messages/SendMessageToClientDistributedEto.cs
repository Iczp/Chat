using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages;

[Serializable]
public class SendMessageToClientDistributedEto : ISendMessageToClientDistributedEto
{
    /// <summary>Required recipient scope for a member-only deletion or clear.</summary>
    public Guid? TargetSessionUnitId { get; set; }
    public Guid? SessionId { get; set; }
    public string LatestChangeSeq { get; set; }
    public string MessageChangeId { get; set; }
    public string ChangeType { get; set; }
    public string ChangeScope { get; set; }
    /// <summary>
    /// Command
    /// </summary>
    public virtual string Command { get; set; }

    /// <summary>
    /// MessageId
    /// </summary>
    public virtual long MessageId { get; set; }

    /// <summary>
    /// CacheKey
    /// </summary>
    public virtual string CacheKey { get; set; }

    /// <summary>
    /// Emiter's HostName
    /// </summary>
    public virtual string HostName { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public virtual MessageInfo<object> Message { get; set; }

    public List<Guid> ReminderIdList { get; set; } = [];

    public List<Guid> FollowerIdList { get; set; } = [];

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{nameof(SendMessageToClientDistributedEto)}]:{nameof(Command)}={Command},{nameof(MessageId)}={MessageId},{nameof(HostName)}={HostName}";
    }
}
