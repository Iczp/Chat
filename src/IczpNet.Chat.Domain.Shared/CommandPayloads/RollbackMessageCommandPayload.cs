using IczpNet.Chat.Commands;
using IczpNet.Pusher.Commands;

namespace IczpNet.Chat.CommandPayloads
{
    [Command(CommandConsts.Rollback)]
    public class RollbackMessageCommandPayload
    {
        public long MessageId { get; set; }
    }
}
