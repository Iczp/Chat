using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.MessageSections.Messages;

[Index(nameof(Id), AllDescending = true)]
[Index(nameof(Id), IsDescending = new[] { false }, Name = "IX_Chat_Message_Id_Asc")]
[Index(nameof(CreationTime), AllDescending = true)]
[Index(nameof(CreationTime), IsDescending = new[] { false }, Name = "IX_Chat_Message_CreationTime_Asc")]
[Index(nameof(SessionUnitCount))]
[Index(nameof(IsPrivate))]
[Index(nameof(MessageType))]
[Index(nameof(IsDeleted))]
[Index(nameof(ForwardMessageId))]
[Index(nameof(QuoteMessageId))]
[Index(nameof(ForwardDepth))]
[Index(nameof(QuoteDepth))]
[Index(nameof(SessionId))]
[Index(nameof(SessionId), nameof(IsPrivate), nameof(SenderId), nameof(ReceiverId), nameof(IsDeleted), nameof(CreationTime), nameof(ForwardDepth), nameof(QuoteDepth))]
public partial class Message : BaseEntity<long>, ISessionId
{
    //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //public virtual long Id { get;  }

    //[Comment("")]
    [StringLength(100)]
    //[Required]
    public virtual string SessionKey { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    [Comment("会话Id")] 
    public virtual Guid? SessionId { get; protected set; }

    /// <summary>
    /// sender session unit
    /// </summary>
    [Comment("会话单元Id")] 
    public virtual Guid? SenderSessionUnitId { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
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
    /// 消息通道
    /// </summary>
    [Required]
    [Comment("消息通道")]
    public virtual Channels Channel { get; protected set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    [Comment("消息类型")]
    public virtual MessageTypes MessageType { get; protected set; }

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

    [NotMapped]
    public virtual bool IsDisabledForward => this.IsDisabledForward();
}
