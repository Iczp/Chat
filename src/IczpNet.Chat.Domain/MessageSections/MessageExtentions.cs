using Castle.DynamicProxy;
using IczpNet.AbpCommons;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System.Collections;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections
{
    public static class MessageExtentions
    {
        /// <summary>
        /// 禁止转发的消息类型
        /// </summary>
        public static List<MessageTypes> DisabledForwardList { get; set; } = new List<MessageTypes>()
        {
            MessageTypes.Cmd,
            MessageTypes.RedEnvelope,
            MessageTypes.Html,
        };

        /// <summary>
        /// 是否禁止转发的消息类型
        /// </summary>
        /// <param name="messageType"></param>
        /// <returns></returns>
        public static bool IsDisabledForward(this MessageTypes messageType) => DisabledForwardList.Contains(messageType);

        public static void SetMessageContent(this Message message, IMessageContentEntity messageContent)
        {
            Assert.NotNull(messageContent, $"MessageContent is null. message:{message}");

            var currentContentType = ProxyUtil.GetUnproxiedType(messageContent);

            var messageType = MessageTemplateAttribute.GetMessageType(currentContentType);

            Assert.NotNull(messageType, $"Item not exists. Key:'{messageType}'");

            message.SetMessageType(messageType);

            var list = message.GetMessageContent();

            var genericType = list.GetType().GetGenericArguments()[0];

            Assert.If(genericType != currentContentType, $"'{currentContentType}' is not of type'{genericType}'");

            var currentInstance = ProxyUtil.GetUnproxiedInstance(messageContent);

            list.Add(currentInstance);

            ContentTypeAttribute.GetPropertyInfo(message.MessageType).SetValue(message, list, null);
        }

        public static IList GetMessageContent(this Message message)
        {
            var propertyInfo = ContentTypeAttribute.GetPropertyInfo(message.MessageType);

            var value = propertyInfo.GetValue(message, null);

            return (IList)value;
        }
    }
}
