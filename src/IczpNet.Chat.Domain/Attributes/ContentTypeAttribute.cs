using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace IczpNet.Chat.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ContentTypeAttribute : Attribute
{
    public static ConcurrentDictionary<MessageTypes, PropertyInfo> MessageTypeDictionary => GenerateDictionary();

    private static ConcurrentDictionary<MessageTypes, PropertyInfo> _messageTypeDictionary;

    public MessageTypes MessageType { get; }

    public ContentTypeAttribute(MessageTypes messageType)
    {
        MessageType = messageType;
    }


    public static ConcurrentDictionary<MessageTypes, PropertyInfo> GenerateDictionary()
    {
        if (_messageTypeDictionary == null)
        {
            _messageTypeDictionary = new ConcurrentDictionary<MessageTypes, PropertyInfo>();
            var props = typeof(Message).GetProperties().Where(x => x.GetCustomAttributes<ContentTypeAttribute>(true).Any());
            foreach (var prop in props)
            {
                var attr = prop.GetCustomAttribute<ContentTypeAttribute>();
                Assert.If(!_messageTypeDictionary.TryAdd(attr.MessageType, prop), $"Item already exists. Key:'{attr.MessageType}',value:'{prop.Name}'");
            }
        }
        return _messageTypeDictionary;
    }

    public static PropertyInfo GetPropertyInfo(MessageTypes messageType)
    {
        Assert.If(!MessageTypeDictionary.TryGetValue(messageType, out PropertyInfo propertyInfo), $"The given key '{messageType}' was not present in the dictionary.");
        return propertyInfo;
    }
    
}
