using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.DataFilters;
using System;

namespace IczpNet.Chat.MessageSections;

public interface IContentEntity : IContent, IIsEnabled, IChatOwner<long?>
{
    Guid Id { get; }

    bool IsVerified { get; }

    string GetBody();

    long GetSize();

    void SetOwnerId(long? ownerId);

    void SetId(Guid guid);
}
