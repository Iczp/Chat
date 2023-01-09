﻿using IczpNet.Pusher.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatPushers
{
    public interface IChatPusher
    {
        Task ExecuteAsync<TCommand>(CommandData commandData);

        Task ExecuteAsync<TCommand>(object data, List<string> ignoreConnections = null);
    }
}