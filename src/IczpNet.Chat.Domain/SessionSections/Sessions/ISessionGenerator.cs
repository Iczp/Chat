using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionGenerator
    {
        Task<Session> MakeAsync(ChatObject sender, ChatObject receiver);

        Task<Session> MakeAsync(ChatObject room);

        Task<List<SessionUnit>> AddShopWaitersIfNotContains(Session session, ChatObject sender, long shopKeeperId);

        Task<List<Session>> GenerateSessionByMessageAsync();
        
    }
}
