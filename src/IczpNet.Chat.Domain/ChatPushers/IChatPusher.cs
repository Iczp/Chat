using IczpNet.Pusher.Commands;
using IczpNet.Pusher.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatPushers
{
    public interface IChatPusher
    {
        Task<long> ExecuteAsync(ChannelMessagePayload commandData);

        Task<long> ExecuteAsync<TCommand>(object data, List<string> ignoreConnections = null);

        Task<long> ExecuteBySessionIdAsync(Guid sessionId, object commandPayload, List<string> ignoreConnections = null);
    }
}
