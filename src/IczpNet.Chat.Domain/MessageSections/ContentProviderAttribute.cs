using IczpNet.AbpCommons;
using IczpNet.Chat.Attributes;
using System;
using System.Reflection;

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


        public static string GetName<T>()
        {
            return GetName(typeof(T));
        }

        public static string GetName(Type type)
        {
            var attribute = type.GetCustomAttribute<ContentProviderAttribute>();
            Assert.NotNull(attribute, $"Non-existent {nameof(MessageTemplateAttribute)} attribute of type:'{type}'.");
            return attribute.ProviderName;
        }
    }
}