using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    protected Message() { }

    public Message(ChatObject sender, ChatObject receiver, string keyName, string keyValue)
    {
        //, IMessageContent messageContent
        Sender = sender;
        SenderType = sender.ObjectType;
        Receiver = receiver;
        ReceiverType = receiver.ObjectType;
        MessageChannel = MessageExtentions.MakeMessageChannel(sender, receiver);
        SessionId = MessageExtentions.MakeSessionId(MessageChannel, sender, receiver);
        //MessageType = messageType;
        KeyName = keyName;
        KeyValue = keyValue;
    }



    internal void SetQuoteMessage(Message quoteMessage)
    {
        QuoteMessage = quoteMessage;
        QuoteDepth = quoteMessage.QuoteDepth + 1;
        QuotePath = quoteMessage.QuotePath + "/" + Id;
    }

    internal virtual void SetKey(string keyName, string keyValue)
    {
        KeyName = keyName;
        KeyValue = keyValue;
    }
}
