using IczpNet.Chat.SessionSections.SessionUnits;
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

        Task<Dictionary<string, long>> ExecuteAsync(object payload, Action<ChannelMessagePayload> action);

        Task<Dictionary<string, long>> ExecuteBySessionIdAsync(Guid sessionId, object commandPayload, List<string> ignoreConnections = null);

        Task<Dictionary<string, long>> ExecutePrivateAsync(List<SessionUnit> sessionUnitList, object commandPayload, List<string> ignoreConnections = null);
    }
}
