using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Auditing;

namespace IczpNet.Chat.MessageSections.Messages;

[Index(nameof(ShortId))]
[Index(nameof(Id), AllDescending = true)]
[Index(nameof(Id), IsDescending = [false], Name = "IX_Chat_Message_Id_Asc")]
[Index(nameof(CreationTime), AllDescending = true)]
[Index(nameof(CreationTime), IsDescending = [false], Name = "IX_Chat_Message_CreationTime_Asc")]
[Index(nameof(SessionUnitCount))]
[Index(nameof(IsPrivate))]
[Index(nameof(MessageType))]
[Index(nameof(IsDeleted))]
[Index(nameof(ForwardMessageId))]
[Index(nameof(ForwardDepth))]
[Index(nameof(ForwardPath))]
[Index(nameof(ForwardCount))]
[Index(nameof(QuoteMessageId))]
[Index(nameof(QuoteDepth))]
[Index(nameof(QuotePath))]
[Index(nameof(QuoteCount))]
[Index(nameof(SenderSessionUnitId))]
[Index(nameof(ReceiverSessionUnitId))]
[Index(nameof(SessionId))]
[Index(nameof(SessionId), nameof(Id), AllDescending = true)]
[Index(nameof(SessionId), nameof(IsPrivate), nameof(SenderId), nameof(ReceiverId), nameof(IsDeleted), nameof(CreationTime), nameof(ForwardDepth), nameof(QuoteDepth))]
[Index(nameof(SessionId), nameof(IsPrivate), nameof(SenderSessionUnitId), nameof(ReceiverSessionUnitId), nameof(IsDeleted), nameof(CreationTime), nameof(ForwardDepth), nameof(QuoteDepth))]
public partial class Message : BaseEntity<long>, ISessionId, IHasEntityVersion
{
    /// <summary>
    /// 实体版本
    /// </summary>
    [Comment("实体版本")]
    public int EntityVersion { get; protected set; }

    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //public virtual long Id { get;  }

    /// <summary>
    /// 短Id
    /// </summary>
    [StringLength(64)]
    [Comment(nameof(ShortId))]
    public virtual string ShortId { get; protected set; }

    /// <summary>
    /// SessionKey
    /// </summary>
    [StringLength(100)]
    [Comment("SessionKey")]
    public virtual string SessionKey { get; protected set; }

    /// <summary>
    /// 会话Id
    /// </summary>
    [Comment("会话Id")]
    public virtual Guid? SessionId { get; protected set; }

    /// <summary>
    /// 接收人会话单元Id(私有消息)
    /// </summary>
    [Comment("接收人会话单元Id(私有消息)")]
    public virtual Guid? ReceiverSessionUnitId { get; protected set; }

    /// <summary>
    /// 会话单元数量
    /// </summary>
    [Comment("会话单元数量")]
    public virtual int SessionUnitCount { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual long ReadedCount => ReadedCounter.Count;

    /// <summary>
    /// 
    /// </summary>
    public virtual long OpenedCount => OpenedCounter.Count;

    /// <summary>
    /// 
    /// </summary>
    public virtual long FavoritedCount => FavoritedCounter.Count;

    /// <summary>
    /// 
    /// </summary>
    public virtual string SenderName => SenderSessionUnit?.DisplayName ?? SenderSessionUnit?.Owner?.Name;

    //public virtual int ReadedCount { get; set; }

    //public virtual int OpenedCount { get; set; }

    //public virtual int FavoritedCount { get; set; }

    [StringLength(100)]
    //[Required]
    [Comment("提供者")]
    public virtual string Provider { get; protected set; }

    /// <summary>
    /// 发送者
    /// </summary>
    [Comment("发送者")]
    public virtual long? SenderId { get; protected set; }

    /// <summary>
    /// 发送者类型
    /// </summary>
    [Comment("发送者类型")]
    public virtual ChatObjectTypeEnums? SenderType { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [NotMapped] 
    public virtual string SenderTypeDescription => SenderType?.GetDescription();

    /// <summary>
    /// 接收者
    /// </summary>
    [Comment("接收者")]
    public virtual long? ReceiverId { get; protected set; }

    /// <summary>
    /// 接收者类型
    /// </summary>
    [Comment("接收者类型")]
    public virtual ChatObjectTypeEnums? ReceiverType { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual string ReceiverTypeDescription => ReceiverType?.GetDescription();

    /// <summary>
    /// 消息通道
    /// </summary>
    [Required]
    [Comment("消息通道")]
    public virtual Channels Channel { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual string ChannelDescription => Channel.GetDescription();

    /// <summary>
    /// 消息类型
    /// </summary>
    [Comment("消息类型")]
    public virtual MessageTypes MessageType { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [NotMapped]
    public virtual string MessageTypeDescription => MessageType.GetDescription();

    /// <summary>
    /// ContentJson
    /// </summary>
    [StringLength(5000)]
    [Comment("ContentJson")]
    public virtual string ContentJson { get; protected set; }

    /// <summary>
    /// 扩展（键名）根据业务自义，如:"courseId"、"course-userId"、"erp-userId"
    /// </summary>
    [StringLength(100)]
    [Comment("扩展（键名）根据业务自义")]
    public virtual string KeyName { get; protected set; }

    /// <summary>
    /// 扩展（键值）根据业务自义,如："123456789"、"02b7d668-02ca-428f-b88c-b8adac2c5044"、"admin"
    /// </summary>
    [StringLength(5000)]
    [Comment("扩展（键值）根据业务自义")]
    public virtual string KeyValue { get; protected set; }

    /// <summary>
    /// 是否撤回
    /// </summary>
    [Comment("是否撤回")]
    public virtual bool IsRollbacked { get; protected set; }

    /// <summary>
    /// 私有消息(只有发送人[senderId]和接收人[receiverId]才能看)
    /// </summary>
    [Comment("私有消息(只有发送人[senderId]和接收人[receiverId]才能看)")]
    public virtual bool IsPrivate { get; protected set; }

    /// <summary>
    /// 指定范围
    /// </summary>
    [Comment("指定范围")]
    public virtual bool IsScoped { get; protected set; }

    /// <summary>
    /// 撤回消息时间
    /// </summary>
    [Comment("撤回消息时间")]
    public virtual DateTime? RollbackTime { get; protected set; }

    /// <summary>
    /// 发送人
    /// </summary>
    [ForeignKey(nameof(SenderId))]
    public virtual ChatObject Sender { get; protected set; }

    /// <summary>
    /// 接收人
    /// </summary>
    [ForeignKey(nameof(ReceiverId))]
    public virtual ChatObject Receiver { get; protected set; }

    /// <summary>
    /// 消息大小kb
    /// </summary>
    [Comment("消息大小kb")]
    public virtual long Size { get; protected set; }

    

    ///// <summary>
    ///// 创建时间/发送时间(UnixTime)
    ///// </summary>
    //[Comment("创建时间/发送时间(UnixTime)")]
    //public virtual long SendTimestamp { get; protected set; } = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();

    /// <summary>
    /// 是否禁止转发的消息类型
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDisabledForward() => MessageExtentions.DisabledForwardList.Contains(MessageType);

    /// <summary>
    /// 是否私有消息
    /// </summary>
    public virtual bool IsPrivateMessage() => IsPrivate && ReceiverSessionUnitId.HasValue;

    /// <summary>
    /// 是否撤回消息
    /// </summary>
    /// <returns></returns>
    public virtual bool IsRollbackMessage() => IsRollbacked || RollbackTime != null;
}
