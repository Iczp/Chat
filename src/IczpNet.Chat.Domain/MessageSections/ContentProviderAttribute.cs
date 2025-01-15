using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using System;
using System.Reflection;

namespace IczpNet.Chat.MessageSections;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ContentProviderAttribute : Attribute
{
    public MessageTypes MessageType { get; }

    public ContentProviderAttribute(MessageTypes messageType)
    {
        MessageType = messageType;
    }

    public static MessageTypes GetMessageType<T>()
    {
        return GetMessageType(typeof(T));
    }

    public static MessageTypes GetMessageType(Type type)
    {
        var attribute = type.GetCustomAttribute<ContentProviderAttribute>();
        Assert.NotNull(attribute, $"Non-existent {nameof(ContentProviderAttribute)} attribute of type:'{type}'.");
        return attribute.MessageType;
    }
}