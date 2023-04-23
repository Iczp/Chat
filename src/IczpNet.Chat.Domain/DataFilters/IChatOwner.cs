using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.ChatObjects;
using System;

namespace IczpNet.Chat.DataFilters
{

    public interface IChatOwner : IChatOwner<long?>
    {
    }

    public interface IChatOwner<TKey> : IOwner<TKey, ChatObject>
    {
    }
}
