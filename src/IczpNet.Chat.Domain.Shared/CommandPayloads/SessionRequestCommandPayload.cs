using IczpNet.Chat.Commands;
using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.CommandPayloads
{
    [Command(CommandConsts.SessionRequest)]
    public class SessionRequestCommandPayload
    {
        public Guid SessionRequestId { get; set; }

        public long OwnerId { get; set; }

        public long DestinationId { get; set; }
    }
}
