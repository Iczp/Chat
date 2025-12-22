using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageSimpleDto : ExtensibleEntityDto, IEntityDto<long>
{
    /// <summary>
    /// 消息Id
    /// </summary>
    public virtual long Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid SessionId { get; set; }

    /// <summary>
    /// 发送人
    /// </summary>
    public virtual long? SenderId { get; set; }

    /// <summary>
    /// 发送人单元
    /// </summary>
    public virtual Guid? SenderSessionUnitId { get; set; }

    /// <summary>
    /// 发送人
    /// </summary>
    public virtual string SenderName { get; set; }

    /// <summary>
    /// 
    /// </summary>

    public virtual long? ReceiverId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid? ReceiverSessionUnitId { get; set; }

    /// <summary>
    ///  消息类型
    /// </summary>
    public virtual MessageTypes MessageType { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public virtual string MessageTypeDescription { get; set; }

    /// <summary>
    /// 提醒类型
    /// </summary>
    public virtual ReminderTypes? ReminderType { get; set; }

    /// <summary>
    /// 提醒类型
    /// </summary>
    public virtual string ReminderTypeDescription { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string KeyName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string KeyValue { get; set; }

    /// <summary>
    /// 是否私有消息
    /// </summary>
    public virtual bool IsPrivate { get; set; }

    /// <summary>
    /// 是否撤回
    /// </summary>
    public virtual bool IsRollbacked { get; set; }

    /// <summary>
    /// 撤回消息时间
    /// </summary>
    public virtual DateTime? RollbackTime { get; set; }

    /// <summary>
    /// 创建时间（发送时间）
    /// </summary>
    public virtual DateTime CreationTime { get; set; }

    //public virtual long SessionUnitCount { get; set; }

    //public virtual long ReadedCount { get; set; }

    //public virtual long OpenedCount { get; set; }

    //public virtual long FavoritedCount { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public virtual object Content { get; set; }
}
