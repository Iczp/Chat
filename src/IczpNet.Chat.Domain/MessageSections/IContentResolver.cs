using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentResolver
    {
        Type GetProviderType(string name);

        Type GetProviderTypeOrDefault(string name);
    }
}
