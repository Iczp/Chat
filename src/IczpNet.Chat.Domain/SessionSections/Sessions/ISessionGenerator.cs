using IczpNet.Chat.ChatObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionGenerator
    {
        Task<Session> MakeAsync(ChatObject sender, ChatObject receiver);

        Task<Session> MakeAsync(ChatObject room);

        Task<List<Session>> GenerateSessionByMessageAsync();
        
    }
}
