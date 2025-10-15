using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageDto : MessageSimpleDto, IEntityDto<long>
{
    //public virtual Guid SessionId { get; set; }

    //public virtual long? SenderId { get; set; }

    //public virtual ChatObjectInfo Sender { get; set; }

    //public virtual long? ReceiverId { get; set; }

    //public virtual string KeyName { get; set; }

    //public virtual string KeyValue { get; set; }

    /// <summary>
    /// 消息内容
    /// </summary>
    public virtual object Content { get; set; }

    /// <summary>
    /// 发送人信息
    /// </summary>
    public virtual SessionUnitSenderDto SenderSessionUnit { get; set; }

}
