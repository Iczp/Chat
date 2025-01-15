using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections;

public interface IContentResolver
{
    Type GetProviderType(MessageTypes messageType);

    Type GetProviderTypeOrDefault(MessageTypes messageType);
}
