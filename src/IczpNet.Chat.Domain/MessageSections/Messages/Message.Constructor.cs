using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    public const int QuotePathMaxLength = 1000;
    public const int ForwardPathMaxLength = 1000;
    public const string Delimiter = "/";
    protected Message() : base() { }

    public Message(IChatObject sender, IChatObject receiver, Session session) : base()
    {
        //, IMessageContent messageContent
        SenderId = sender.Id;
        SenderType = sender.ObjectType;
        ReceiverId = receiver.Id;
        ReceiverType = receiver.ObjectType;
        Channel = session.Channel;
        Session = session;
        SessionKey = session.SessionKey;
        //SessionUnitCount = session.GetMemberCount();
        //MessageType = messageType;
    }

    public Message(SessionUnit sessionUnit) : base()
    {
        SessionUnit = sessionUnit;
        SenderId = sessionUnit.OwnerId;
        SenderType = sessionUnit.Owner?.ObjectType;
        ReceiverId = sessionUnit.DestinationId;
        ReceiverType = sessionUnit.DestinationObjectType;

        Session = sessionUnit.Session;
        Channel = Session.Channel;
        SessionKey = Session.SessionKey;
    }

    public virtual void SetQuoteMessage(Message source)
    {
        QuoteMessage = source;
        QuoteDepth = source.QuoteDepth + 1;
        QuotePath = source.QuotePath + Delimiter + source.Id;
        Assert.If(QuotePath.Length > QuotePathMaxLength, "Maximum length exceeded in [QuotePath].");
    }

    public virtual void SetForwardMessage(Message source)
    {
        ForwardMessage = source;
        ForwardDepth = source.ForwardDepth + 1;
        ForwardPath = source.ForwardPath + Delimiter + source.Id;
        Assert.If(ForwardPath.Length > ForwardPathMaxLength, "Maximum length exceeded in [ForwardPath].");
    }

    public virtual void SetKey(string keyName, string keyValue)
    {
        KeyName = keyName;
        KeyValue = keyValue;
    }

    public virtual void SetContentJson(string contentJson)
    {
        ContentJson = contentJson;
    }

    public virtual void SetMessageType(MessageTypes messageType)
    {
        MessageType = messageType;
    }

    public virtual void SetSize(long size)
    {
        Size = size;
    }

    public void SetContentProvider(Type type)
    {
        Provider = ContentProviderAttribute.GetName(type);
    }

    public void SetContentProvider<T>()
    {
        Provider = ContentProviderAttribute.GetName<T>();
    }

    internal void Rollback(DateTime now)
    {
        IsRollbacked = true;
        RollbackTime = now;
    }

    internal void SetPrivateMessage(ChatObject receiver)
    {
        ReceiverId = receiver.Id;
        ReceiverType = receiver.ObjectType;
        IsPrivate = true;
    }

    [NotMapped]
    public virtual bool? IsOpened { get; set; }

    [NotMapped]
    public virtual bool? IsReaded { get; set; }

    [NotMapped]
    public virtual bool? IsFavorited { get; set; }

    [NotMapped]
    public virtual bool? IsFollowing { get; set; }

    
}
