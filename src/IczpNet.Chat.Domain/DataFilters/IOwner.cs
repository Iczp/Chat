using IczpNet.Chat.ChatObjects;
using System;

namespace IczpNet.Chat.DataFilters
{
    public interface IOwner
    {
        ChatObject Owner { get; }
        Guid OwnerId { get; }
    }
}
