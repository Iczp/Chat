using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.MessageSections
{
    public interface IContentResolver
    {
        string GetProvider(string name);
    }
}
