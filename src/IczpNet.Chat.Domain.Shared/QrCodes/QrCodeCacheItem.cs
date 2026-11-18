using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.QrCodes;

[Serializable]
public class QrCodeCacheItem
{
    public virtual Guid Id { get; set; }

    public virtual string Code { get; set; }

    public virtual QrCodeScenes Scene { get; set; }

    public virtual long? OwnerId { get; set; }

    public virtual ChatObjectTypeEnums? OwnerObjectType { get; set; }

    public virtual long? InviterId { get; set; }

    public virtual string Title { get; set; }

    public virtual string Content { get; set; }

    public virtual bool IsPermanent { get; set; }

    public virtual int? ExpireDays { get; set; }

    public virtual DateTime? ExpirationTime { get; set; }

    public virtual bool IsEnabled { get; set; }

    public virtual int? MaxUseCount { get; set; }

    public virtual int UsedCount { get; set; }

    public virtual string PayloadJson { get; set; }

    public virtual DateTime CreationTime { get; set; }

    public bool IsExpired(DateTime now)
        => !IsPermanent && ExpirationTime.HasValue && ExpirationTime.Value <= now;

    public bool IsReachMaxUseCount()
        => MaxUseCount.HasValue && MaxUseCount.Value > 0 && UsedCount >= MaxUseCount.Value;
}
