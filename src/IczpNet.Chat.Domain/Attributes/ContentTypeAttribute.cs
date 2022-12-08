using IczpNet.Chat.Enums;
using System;
using System.Reflection;

namespace IczpNet.Chat.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ContentTypeAttribute : Attribute
    {
        public MessageTypes MessageType { get; }

        public ContentTypeAttribute(MessageTypes messageType)
        {
            MessageType = messageType;
        }

        //public static ChatObjectTypes GetCommandName<T>()
        //{
        //    return GetCommandName(typeof(T));
        //}

        //public static ChatObjectTypes GetCommandName(Type type)
        //{
        //    var nameAttribute = type.GetCustomAttribute<ObjectTypeAttribute>();

        //    if (nameAttribute == null)
        //    {
        //        return type.FullName;
        //    }
        //    return nameAttribute.GetObjectType(type);
        //}
    }
}
