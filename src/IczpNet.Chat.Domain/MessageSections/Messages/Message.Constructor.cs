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
        Receiver = receiver;
        SessionId = MakeSessionId(sender, receiver);
        MessageChannel = MakeMessageChannel(sender, receiver);
        //MessageType = messageType;
        KeyName = keyName;
        KeyValue = keyValue;
    }

    protected virtual MessageChannels MakeMessageChannel(ChatObject sender, ChatObject receiver)
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
            return MessageChannels.RoomChannel;
        }


        return MessageChannels.PersonalChannel;
    }

    protected virtual string MakeSessionId(ChatObject sender, ChatObject receiver)
    {
        throw new NotImplementedException();
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
