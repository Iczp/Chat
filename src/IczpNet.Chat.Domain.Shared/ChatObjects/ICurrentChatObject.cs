using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatObjects
{
    public interface ICurrentChatObject
    {
        long Id { get; }
        //long AutoId { get; }
        string Name { get; }
        Guid? OwnerUserId { get;  }
        ChatObjectTypeEnums? ObjectType { get;  }
        long? GetId();
    }
}
