using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace IczpNet.Chat.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MessageTemplateAttribute : Attribute
    {
        public static ConcurrentDictionary<Type, MessageTypes> MessageTemplateDictionary => GenerateDictionary();

        private static ConcurrentDictionary<Type, MessageTypes> _messageTemplateDictionary;

        public MessageTypes MessageType { get; }

        public MessageTemplateAttribute(MessageTypes messageType)
        {
            MessageType = messageType;
        }

        public static ConcurrentDictionary<Type, MessageTypes> GenerateDictionary()
        {
            if (_messageTemplateDictionary == null)
            {
                _messageTemplateDictionary = new ConcurrentDictionary<Type, MessageTypes>();

                var entityTypes = typeof(IMessageContentEntity).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IMessageContentEntity).IsAssignableFrom(x)));

                foreach (var entityType in entityTypes)
                {
                    var attribute = entityType.GetCustomAttribute<MessageTemplateAttribute>();
                    if (attribute != null)
                    {
                        Assert.If(!_messageTemplateDictionary.TryAdd(entityType, attribute.MessageType), $"Item already exists. Key:'{entityType}',value:'{attribute.MessageType}'");
                    }
                }
            }
            return _messageTemplateDictionary;
        }

        public static MessageTypes GetMessageType<T>()
        {
            return GetMessageType(typeof(T));
        }

        public static MessageTypes GetMessageType(Type type)
        {
            var nameAttribute = type.GetCustomAttribute<MessageTemplateAttribute>();

            Assert.NotNull(nameAttribute, $"Item already exists. Key:'{type}',value:'{nameAttribute.MessageType}'");

            return nameAttribute.MessageType;

            //return MessageTemplateDictionary[type];
        }
    }
}
