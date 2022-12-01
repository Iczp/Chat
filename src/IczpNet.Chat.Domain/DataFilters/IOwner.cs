using IczpNet.Chat.ChatObjects;
using System;

namespace IczpNet.Chat.DataFilters
{


    public interface IOwner<TKey> : IOwner //where TKey : struct
    {
        TKey OwnerId { get; }
    }

    public interface IOwner
    {
        ChatObject Owner { get; }
    }
}
