﻿using IczpNet.Pusher.Commands;
using System;

namespace IczpNet.Chat.CommandPayloads
{
    [Command("Rollback")]
    public class RollbackMessageCommandPayload
    {
        public long MessageId { get; set; }
    }
}