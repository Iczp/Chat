using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.Sessions;
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
        SenderSessionUnit = sessionUnit;
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

    internal void Rollback(DateTime now)
    {
        IsRollbacked = true;
        RollbackTime = now;
    }

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
