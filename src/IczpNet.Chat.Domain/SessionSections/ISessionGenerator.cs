using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections
{
    public interface ISessionGenerator
    {
        Task<Session> MakeAsync(MessageChannels messageChannel, ChatObject sender, ChatObject receiver);
        Task<List<Session>> GenerateAsync(Guid ownerId, long? startMessageAutoId = null);
    }
}
