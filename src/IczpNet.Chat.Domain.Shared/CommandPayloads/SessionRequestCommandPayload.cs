using IczpNet.Chat.Commands;
using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.CommandPayloads
{
    [Command(CommandConsts.SessionRequest)]
    [Serializable]
    public class SessionRequestCommandPayload
    {
        public virtual Guid SessionRequestId { get; set; }

        public virtual long OwnerId { get; set; }

        public virtual long DestinationId { get; set; }
    }
}
