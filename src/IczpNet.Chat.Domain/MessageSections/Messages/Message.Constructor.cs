using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    public const int QuotePathMaxLength = 1000;
    public const int ForwardPathMaxLength = 1000;
    public const string Delimiter = "/";
    protected Message() : base() { }

    public Message(SessionUnit sessionUnit) : base()
    {
        //SenderSessionUnit = sessionUnit;
        SenderId = sessionUnit.OwnerId;
        SenderType = sessionUnit.OwnerObjectType;
        ReceiverId = sessionUnit.DestinationId;
        ReceiverType = sessionUnit.DestinationObjectType;
        //Session = sessionUnit.Session
        SenderSessionUnitId = sessionUnit.Id;
        SessionId = sessionUnit.SessionId;
        //Channel = sessionUnit.Session.Channel;
        SessionKey = sessionUnit.Session.SessionKey;
    }

    /// <summary>
    /// 设置短Id
    /// </summary>
    /// <param name="shortId"></param>
    public virtual void SetShortId(string shortId)
    {
        ShortId = shortId;
    }

    /// <summary>
    /// 设置引用消息
    /// </summary>
    /// <param name="source"></param>
    public virtual void SetQuoteMessage(Message source)
    {
        //QuoteMessage = source;
        QuoteDepth = source.QuoteDepth + 1;
        QuotePath = source.QuotePath + Delimiter + source.Id;
        QuoteMessageId = source.Id;
        Assert.If(QuotePath.Length > QuotePathMaxLength, "Maximum length exceeded in [QuotePath].");
    }

    /// <summary>
    /// 设置转发消息
    /// </summary>
    /// <param name="source"></param>
    public virtual void SetForwardMessage(Message source)
    {
        //ForwardMessage = source;
        ForwardDepth = source.ForwardDepth + 1;
        ForwardPath = source.ForwardPath + Delimiter + source.Id;
        ForwardMessageId = source.Id;
        Assert.If(ForwardPath.Length > ForwardPathMaxLength, "Maximum length exceeded in [ForwardPath].");
    }

    /// <summary>
    /// 设置键值对
    /// </summary>
    /// <param name="keyName"></param>
    /// <param name="keyValue"></param>
    public virtual void SetKey(string keyName, string keyValue)
    {
        KeyName = keyName;
        KeyValue = keyValue;
    }

    /// <summary>
    /// 设置内容Json
    /// </summary>
    /// <param name="contentJson"></param>
    public virtual void SetContentJson(string contentJson)
    {
        ContentJson = contentJson;
    }

    /// <summary>
    /// 设置消息类型
    /// </summary>
    /// <param name="messageType"></param>
    public virtual void SetMessageType(MessageTypes messageType)
    {
        MessageType = messageType;
    }

    /// <summary>
    /// 设置大小
    /// </summary>
    /// <param name="size"></param>
    public virtual void SetSize(long size)
    {
        Size = size;
    }

    /// <summary>
    /// 撤回消息
    /// </summary>
    /// <param name="now"></param>
    internal void Rollback(DateTime now)
    {
        IsRollbacked = true;
        RollbackTime = now;
    }

    /// <summary>
    /// 设置私有消息
    /// </summary>
    /// <param name="receiverSessionUnitId"></param>
    internal void SetPrivateMessage(Guid receiverSessionUnitId)
    {
        //ReceiverSessionUnit = receiverSessionUnit;
        //ReceiverId = receiverSessionUnit.OwnerId;
        //ReceiverType = receiverSessionUnit.OwnerObjectType;
        ReceiverSessionUnitId = receiverSessionUnitId;
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

    /// <summary>
    /// 朋友关系Id
    /// </summary>
    [NotMapped]
    public virtual Guid? FriendshipSessionUnitId { get; set; }

    /// <summary>
    /// 发送人显示名称
    /// </summary>
    [NotMapped]
    public virtual string SenderDisplayName { get; set; }

    public override string ToString()
    {
        return $"Id={Id},MessageType={MessageType},SenderId={SenderId},ReceiverId={ReceiverId},SenderSessionUnitId={SenderSessionUnitId}";
    }
}
