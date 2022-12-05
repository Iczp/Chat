﻿namespace IczpNet.Chat.DataFilters
{
    public interface IOwner<TKey, T> : IOwner<TKey>, IOwnerObject<T>
    {
    }

    public interface IOwner<TKey>
    {
        TKey OwnerId { get; }
    }
}
