using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IChatObjectResolver
    {
        Task<List<ChatObject>> GetListAsync(Message message);
        Task<IEnumerable<Guid>> GetIdListAsync(Message message);
    }
}
