using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatObjects
{
    public interface ICurrentChatObject
    {
        Guid Id { get; }
        long AutoId { get; }
        string Name { get; }
        Guid? OwnerUserId { get;  }
        ChatObjectTypes? ObjectType { get;  }
        Guid? GetId();
    }
}
