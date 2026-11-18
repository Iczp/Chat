using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.QrCodes;

/// <summary>
/// 通用二维码（名片、群邀请、工作台应用等）
/// </summary>
public class QrCode : BaseEntity<Guid>, IIsEnabled, IChatOwner<long?>
{
    /// <summary>
    /// 唯一短码
    /// </summary>
    [Comment("唯一短码")]
    [Required]
    [MaxLength(64)]
    public virtual string Code { get; protected set; }

    /// <summary>
    /// 二维码场景类型
    /// </summary>
    [Comment("二维码场景类型")]
    public virtual QrCodeScenes Scene { get; protected set; }

    /// <summary>
    /// 标题/备注名称
    /// </summary>
    [Comment("标题")]
    [MaxLength(200)]
    public virtual string Title { get; protected set; }

    /// <summary>
    /// 二维码内容（模板渲染后的URI/文本）
    /// </summary>
    [Comment("二维码内容")]
    [MaxLength(1024)]
    public virtual string Content { get; protected set; }

    /// <summary>
    /// 所属聊天对象Id（名片对象或群聊Id）
    /// </summary>
    [Comment("所属聊天对象Id")]
    public virtual long? OwnerId { get; protected set; }

    /// <summary>
    /// 所属聊天对象
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    /// <summary>
    /// 所属聊天对象类型
    /// </summary>
    [Comment("所属聊天对象类型")]
    public virtual ChatObjectTypeEnums? OwnerObjectType { get; protected set; }

    /// <summary>
    /// 邀请人/分享人聊天对象Id
    /// </summary>
    [Comment("邀请人聊天对象Id")]
    public virtual long? InviterId { get; protected set; }

    /// <summary>
    /// 邀请人/分享人
    /// </summary>
    [ForeignKey(nameof(InviterId))]
    public virtual ChatObject Inviter { get; protected set; }

    /// <summary>
    /// 是否永久有效
    /// </summary>
    [Comment("是否永久有效")]
    public virtual bool IsPermanent { get; protected set; }

    /// <summary>
    /// 有效天数（永久为null）
    /// </summary>
    [Comment("有效天数")]
    public virtual int? ExpireDays { get; protected set; }

    /// <summary>
    /// 过期时间（UTC，永久为null）
    /// </summary>
    [Comment("过期时间")]
    public virtual DateTime? ExpirationTime { get; protected set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    [Comment("是否可用")]
    public virtual bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 最大使用次数（null 或小于等于 0 表示不限制）
    /// </summary>
    [Comment("最大使用次数")]
    public virtual int? MaxUseCount { get; protected set; }

    /// <summary>
    /// 已使用次数
    /// </summary>
    [Comment("已使用次数")]
    public virtual int UsedCount { get; protected set; }

    /// <summary>
    /// 扩展业务载荷JSON
    /// </summary>
    [Comment("扩展业务载荷JSON")]
    [MaxLength(2000)]
    public virtual string PayloadJson { get; protected set; }

    protected QrCode()
    {
    }

    public QrCode(Guid id, string code, QrCodeScenes scene) : base(id)
    {
        SetCode(code);
        Scene = scene;
        IsEnabled = true;
    }

    public void SetCode(string code)
    {
        Code = code;
    }

    public void SetContent(string content)
    {
        Content = content;
    }

    public void SetScene(QrCodeScenes scene)
    {
        Scene = scene;
    }

    public void SetOwner(long? ownerId, ChatObjectTypeEnums? ownerObjectType = null)
    {
        OwnerId = ownerId;
        OwnerObjectType = ownerObjectType;
    }

    public void SetInviterId(long? inviterId)
    {
        InviterId = inviterId;
    }

    public void SetValidity(bool isPermanent, int? expireDays, DateTime? expirationTime)
    {
        IsPermanent = isPermanent;
        ExpireDays = isPermanent ? null : expireDays;
        ExpirationTime = isPermanent ? null : expirationTime;
    }

    public void SetTitle(string title)
    {
        Title = title;
    }

    public void SetMaxUseCount(int? maxUseCount)
    {
        MaxUseCount = maxUseCount;
    }

    public void SetPayloadJson(string payloadJson)
    {
        PayloadJson = payloadJson;
    }

    public void IncrementUsedCount()
    {
        UsedCount++;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public bool IsExpired(DateTime now)
        => !IsPermanent && ExpirationTime.HasValue && ExpirationTime.Value <= now;

    public bool IsReachMaxUseCount()
        => MaxUseCount.HasValue && MaxUseCount.Value > 0 && UsedCount >= MaxUseCount.Value;
}
