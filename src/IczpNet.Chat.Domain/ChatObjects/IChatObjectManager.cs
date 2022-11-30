using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatObjects
{
    public interface IChatObjectManager
    {
        Task<List<ChatObject>> GetListByUserId(Guid userId);
    }
}
