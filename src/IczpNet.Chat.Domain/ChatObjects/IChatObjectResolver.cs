using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjects;

public interface IChatObjectResolver
{
    Task<List<ChatObject>> GetListAsync(Message message);
    Task<List<long>> GetIdListAsync(Message message);
}
