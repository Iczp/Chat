using IczpNet.Chat.ChatObjects;
using System;

namespace IczpNet.Chat.DataFilters
{

    public interface IChatOwner : IChatOwner<Guid>
    {
    }

    public interface IChatOwner<TKey> : IOwner<TKey, ChatObject>
    {
    }
}
