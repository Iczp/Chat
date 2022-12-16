using IczpNet.AbpCommons;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    public const int QuotePathMaxLength = 1000;
    public const int ForwardPathMaxLength = 1000;
    public const string Delimiter = "/";
    protected Message() { }

    public Message(Guid id, MessageChannels messageChannel, ChatObject sender, ChatObject receiver, Session session) : base(id)
    {
        //, IMessageContent messageContent
        Sender = sender;
        SenderType = sender.ObjectType;
        Receiver = receiver;
        ReceiverType = receiver.ObjectType;
        MessageChannel = messageChannel;
        Session= session;
        SessionKey = session.SessionKey;
        //MessageType = messageType;
    }

    public virtual void SetQuoteMessage(Message source)
    {
        QuoteMessage = source;
        QuoteDepth = source.QuoteDepth + 1;
        QuotePath = source.QuotePath + Delimiter + source.AutoId;
        Assert.If(QuotePath.Length > QuotePathMaxLength, "Maximum length exceeded in [QuotePath].");
    }

    public virtual void SetForwardMessage(Message source)
    {
        ForwardMessage = source;
        ForwardDepth = source.QuoteDepth + 1;
        ForwardPath = source.QuotePath + Delimiter + source.AutoId;
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

    /// <summary>
    /// 设置提醒所有人 @Everyone
    /// </summary>
    public virtual void SetRemindAll()
    {
        SetKey(MessageKeyNames.Remind, MessageKeyNames.RemindEveryone);
    }

    /// <summary>
    /// 设置提醒用户 @ChatObjectId
    /// </summary>
    /// <param name="chatObjectIdList"></param>
    public virtual void SetRemindChatObject(List<Guid> chatObjectIdList)
    {
        SetKey(MessageKeyNames.Remind, string.Join(",", chatObjectIdList.ToArray()));
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
}
