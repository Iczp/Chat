using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.QrCodes;

[Serializable]
public class QrCodeScanDataInfo
{
    public virtual Guid? QrCodeId { get; set; }

    public virtual string Code { get; set; }

    public virtual QrCodeScenes Scene { get; set; }

    /// <summary>
    /// 语义动作标识: chat_object_card / group_invite / workbench_app / custom_payload
    /// </summary>
    public virtual string Action { get; set; }

    public virtual bool IsValid { get; set; }

    public virtual bool IsExpired { get; set; }

    public virtual bool IsDisabled { get; set; }

    public virtual string InvalidReason { get; set; }

    public virtual bool IsPermanent { get; set; }

    public virtual int? ExpireDays { get; set; }

    public virtual DateTime? ExpirationTime { get; set; }

    public virtual long? OwnerId { get; set; }

    public virtual string OwnerName { get; set; }

    public virtual string OwnerCode { get; set; }

    public virtual string OwnerPortrait { get; set; }

    public virtual string OwnerThumbnail { get; set; }

    public virtual ChatObjectTypeEnums? OwnerObjectType { get; set; }

    public virtual string OwnerObjectTypeDescription { get; set; }

    public virtual Genders? Gender { get; set; }

    public virtual string Description { get; set; }

    public virtual VerificationMethods? VerificationMethod { get; set; }

    public virtual bool NeedsApproval { get; set; }

    public virtual long? InviterId { get; set; }

    public virtual string InviterName { get; set; }

    public virtual int? MemberCount { get; set; }

    public virtual long? ViewerChatObjectId { get; set; }

    public virtual bool IsSelf { get; set; }

    public virtual bool IsFriend { get; set; }

    public virtual bool IsAlreadyMember { get; set; }

    public virtual Guid? SessionUnitId { get; set; }

    public virtual Guid? SessionId { get; set; }

    public virtual string PayloadJson { get; set; }
}
