using IczpNet.Chat.Enums;
using System;
using System.Reflection;

namespace IczpNet.Chat.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MessageTemplateAttribute : Attribute
    {
        public MessageTypes MessageType { get; }

        public MessageTemplateAttribute(MessageTypes messageType)
        {
            MessageType = messageType;
        }



        //public static MessageTypes GetName<T>()
        //{
        //    return GetName(typeof(T));
        //}

        //public static MessageTypes GetName(Type type)
        //{
        //    var nameAttribute = type.GetCustomAttribute<MessageTemplateAttribute>();

        //    if (nameAttribute == null)
        //    {
        //        return type.FullName;
        //    }
        //    return nameAttribute.GetObjectType(type);
        //}
    }
}
