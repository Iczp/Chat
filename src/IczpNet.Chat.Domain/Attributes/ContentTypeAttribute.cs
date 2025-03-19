using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IczpNet.Chat.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ContentTypeAttribute(MessageTypes messageType) : Attribute
{
    // 使用 Lazy<T> 确保 _messageTypeDictionary 只初始化一次
    private static readonly Lazy<ConcurrentDictionary<MessageTypes, PropertyInfo>> _lazyMessageTypeDictionary
        = new(() => GenerateDictionary());

    public static ConcurrentDictionary<MessageTypes, PropertyInfo> MessageTypeDictionary => _lazyMessageTypeDictionary.Value;

    public MessageTypes MessageType { get; } = messageType;

    private static ConcurrentDictionary<MessageTypes, PropertyInfo> GenerateDictionary()
    {
        var dictionary = new ConcurrentDictionary<MessageTypes, PropertyInfo>();

        var props = typeof(Message).GetProperties()
            .Where(x => x.GetCustomAttributes<ContentTypeAttribute>(true).Any());

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<ContentTypeAttribute>();
            if (attr == null)
            {
                continue; // 避免 null 访问问题
            }

            if (!dictionary.TryAdd(attr.MessageType, prop))
            {
                throw new InvalidOperationException($"Item already exists. Key:'{attr.MessageType}', value:'{prop.Name}'");
            }
        }

        return dictionary;
    }

    public static PropertyInfo GetPropertyInfo(MessageTypes messageType)
    {
        if (!MessageTypeDictionary.TryGetValue(messageType, out PropertyInfo propertyInfo))
        {
            throw new KeyNotFoundException($"The given key '{messageType}' was not present in the dictionary.");
        }
        return propertyInfo;
    }
}
