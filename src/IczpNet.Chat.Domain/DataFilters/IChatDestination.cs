using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.ChatObjects;
using System;

namespace IczpNet.Chat.DataFilters;

public interface IChatDestination<TKey> : IDestination<TKey, ChatObject>, IChatDestination
{
}

public interface IChatDestination
{
}
