using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IczpNet.Chat.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class MessageTemplateAttribute(MessageTypes messageType) : Attribute
{
    private static readonly Lazy<ConcurrentDictionary<Type, MessageTypes>> _lazyMessageTemplateDictionary
        = new(() => GenerateDictionary());

    public static ConcurrentDictionary<Type, MessageTypes> MessageTemplateDictionary => _lazyMessageTemplateDictionary.Value;

    public MessageTypes MessageType { get; } = messageType;

    private static ConcurrentDictionary<Type, MessageTypes> GenerateDictionary()
    {
        var dictionary = new ConcurrentDictionary<Type, MessageTypes>();

        var entityTypes = typeof(IContentEntity).Assembly.GetExportedTypes()
            .Where(t => !t.IsAbstract && typeof(IContentEntity).IsAssignableFrom(t));

        foreach (var entityType in entityTypes)
        {
            var attribute = entityType.GetCustomAttribute<MessageTemplateAttribute>();
            if (attribute != null)
            {
                if (!dictionary.TryAdd(entityType, attribute.MessageType))
                {
                    throw new InvalidOperationException($"Item already exists. Key:'{entityType}', value:'{attribute.MessageType}'");
                }
            }
        }

        return dictionary;
    }

    public static MessageTypes GetMessageType<T>()
    {
        return GetMessageType(typeof(T));
    }

    public static MessageTypes GetMessageType(Type type)
    {
        if (!MessageTemplateDictionary.TryGetValue(type, out MessageTypes messageType))
        {
            throw new KeyNotFoundException($"The given type '{type}' does not have a MessageTemplateAttribute.");
        }

        return messageType;
    }
}
