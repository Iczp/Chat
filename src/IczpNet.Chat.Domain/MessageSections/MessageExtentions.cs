using IczpNet.AbpCommons;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.MessageSections
{
    public static class MessageExtentions
    {

        public static ConcurrentDictionary<MessageTypes, PropertyInfo> MessageTypeDictionary => GenerateMessageTypeDictionary();

        private static ConcurrentDictionary<MessageTypes, PropertyInfo> _messageTypeDictionary;

        public static ConcurrentDictionary<Type, MessageTypes> MessageTemplateDictionary => GenerateMessageTemplateDictionary();

        private static ConcurrentDictionary<Type, MessageTypes> _messageTemplateDictionary;

        public static MessageChannels MakeMessageChannel(ChatObject sender, ChatObject receiver)
        {
            if (sender.ObjectType == ChatObjectTypes.Room || receiver.ObjectType == ChatObjectTypes.Room)
            {
                return MessageChannels.RoomChannel;
            }
            else if (sender.ObjectType == ChatObjectTypes.Official || receiver.ObjectType == ChatObjectTypes.Official)
            {
                return MessageChannels.ServiceChannel;
            }
            else if (sender.ObjectType == ChatObjectTypes.Square || receiver.ObjectType == ChatObjectTypes.Square)
            {
                return MessageChannels.SquareChannel;
            }

            return MessageChannels.PersonalToPersonal;
        }

        public static string MakeSessionId(MessageChannels messageChannel, ChatObject Sender, ChatObject Receiver, bool IsReverse = false)
        {
            string sessionId = null;
            switch (messageChannel)
            {
                case MessageChannels.RoomChannel:
                case MessageChannels.SubscriptionChannel:
                case MessageChannels.ServiceChannel:
                case MessageChannels.SquareChannel:
                    sessionId = Receiver.Id.ToString();
                    break;
                case MessageChannels.PersonalToPersonal:
                case MessageChannels.RobotChannel:
                    var arr = new[] { Sender.Id, Receiver.Id };
                    Array.Sort(arr);
                    sessionId = string.Join(":", arr);
                    break;

                case MessageChannels.ElectronicCommerceChannel:
                    break;
            }
            return sessionId;
        }




        public static ConcurrentDictionary<Type, MessageTypes> GenerateMessageTemplateDictionary()
        {
            if (_messageTemplateDictionary == null)
            {
                _messageTemplateDictionary = new ConcurrentDictionary<Type, MessageTypes>();

                var entityTypes = typeof(IMessageContent).Assembly.GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.GetInterfaces().Any(x => typeof(IMessageContent).IsAssignableFrom(x)));

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

        public static ConcurrentDictionary<MessageTypes, PropertyInfo> GenerateMessageTypeDictionary()
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

        public static void SetMessageContent(this Message message, IMessageContent messageContent)
        {
            var list = message.GetMessageContent();

            list.Add(messageContent);

            var messageType= MessageTemplateDictionary.GetValueOrDefault(messageContent.GetType());

            Assert.NotNull(messageType, $"Item not exists. Key:'{messageType}'");

            message.SetMessageType(messageType);

            MessageTypeDictionary[message.MessageType].SetValue(message, list, null);
        }

        public static IList GetMessageContent(this Message message)
        {
            Assert.If(!MessageTypeDictionary.TryGetValue(message.MessageType, out PropertyInfo propertyInfo), $"The given key '{message.MessageType}' was not present in the dictionary.");
            return (IList)propertyInfo.GetValue(message, null);
        }
    }
}
