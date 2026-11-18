using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.QrCodes.Dtos;

public class QrCodeDto : BaseDto<Guid>
{
    public virtual string Code { get; set; }

    public virtual QrCodeScenes Scene { get; set; }

    public virtual string Title { get; set; }

    public virtual string Content { get; set; }

    public virtual long? OwnerId { get; set; }

    public virtual ChatObjectTypeEnums? OwnerObjectType { get; set; }

    public virtual long? InviterId { get; set; }

    public virtual bool IsPermanent { get; set; }

    public virtual int? ExpireDays { get; set; }

    public virtual DateTime? ExpirationTime { get; set; }

    public virtual bool IsEnabled { get; set; }

    public virtual int? MaxUseCount { get; set; }

    public virtual int UsedCount { get; set; }

    public virtual string PayloadJson { get; set; }
}
