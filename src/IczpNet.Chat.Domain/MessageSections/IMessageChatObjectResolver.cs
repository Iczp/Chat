using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections
{
    public interface IMessageChatObjectResolver
    {
        Task<List<ChatObject>> GetChatObjectListAsync(Message message);
        Task<IEnumerable<Guid>> GetChatObjectIdListAsync(Message message);
    }
}
