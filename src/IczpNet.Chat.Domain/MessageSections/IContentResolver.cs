using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentResolver
    {
        Type GetProviderType(MessageTypes messageType);

        Type GetProviderTypeOrDefault(MessageTypes messageType);
    }
}
