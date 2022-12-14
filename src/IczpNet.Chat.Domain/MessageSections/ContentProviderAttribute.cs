using System;

namespace IczpNet.Chat.MessageSections
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ContentProviderAttribute : Attribute
    {
        public string ProviderName { get; }

        public ContentProviderAttribute(string providerName)
        {
            ProviderName = providerName;
        }
    }
}