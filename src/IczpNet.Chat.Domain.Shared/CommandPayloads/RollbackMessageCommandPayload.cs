using IczpNet.Chat.Commands;
using System;

namespace IczpNet.Chat.CommandPayloads;

[Command(CommandConsts.Rollback)]
[Serializable]
public class RollbackMessageCommandPayload //: MessageRollbackEto
{
    public virtual long MessageId { get; set; }
}
