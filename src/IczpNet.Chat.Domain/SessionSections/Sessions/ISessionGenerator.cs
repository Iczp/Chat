using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionGenerator
    {
        Task<Session> MakeAsync(ChatObject sender, ChatObject receiver);
        Task<List<Session>> GenerateSessionByMessageAsync();
    }
}
