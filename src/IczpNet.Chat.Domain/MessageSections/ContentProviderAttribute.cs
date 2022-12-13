using System;

namespace IczpNet.Chat.MessageSections
{
    public class ContentProviderAttribute : Attribute
    {
        public string ProviderName { get; }

        public ContentProviderAttribute(string providerName)
        {
            ProviderName = ProviderName;
        }
    }
}