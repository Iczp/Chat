using IczpNet.Chat.ChatObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionGenerator
    {
        Task<Session> MakeAsync(ChatObjectInfo sender, ChatObjectInfo receiver);

        Task<List<Session>> GenerateSessionByMessageAsync();

        Task<Session> UpdateAsync(Session session);
    }
}
