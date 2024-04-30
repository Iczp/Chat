using IczpNet.Chat.Commands;
using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.CommandPayloads;

[Command(CommandConsts.IncrementCompleted)]
[Serializable]
public class IncrementCompletedCommandPayload
{
    public virtual long MessageId { get; set; }
}
