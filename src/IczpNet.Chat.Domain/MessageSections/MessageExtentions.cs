using IczpNet.AbpCommons;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.MessageSections.Messages;
using System.Collections;

namespace IczpNet.Chat.MessageSections
{
    public static class MessageExtentions
    {

        public static void SetMessageContent(this Message message, IMessageContent messageContent)
        {
            var messageType = MessageTemplateAttribute.GetMessageType(messageContent.GetType());

            Assert.NotNull(messageType, $"Item not exists. Key:'{messageType}'");

            message.SetMessageType(messageType);

            var list = message.GetMessageContent();

            var genericType = list.GetType().GetGenericArguments()[0];

            Assert.If(genericType != messageContent.GetType(), $"'{messageContent.GetType()}' is not of type'{genericType}'");

            list.Add(messageContent);

            ContentTypeAttribute.GetPropertyInfo(message.MessageType).SetValue(message, list, null);
        }

        public static IList GetMessageContent(this Message message)
        {
            return (IList)ContentTypeAttribute.GetPropertyInfo(message.MessageType).GetValue(message, null);
        }
    }
}
