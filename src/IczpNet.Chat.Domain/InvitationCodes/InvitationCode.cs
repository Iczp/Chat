using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.InvitationCodes;

/// <summary>
/// 邀请码
/// </summary>
[Index(nameof(Code))]
public class InvitationCode : BaseEntity<Guid>, IIsEnabled
{
    /// <summary>
    /// 标题
    /// </summary>
    [Comment("标题")]
    [MaxLength(200)]
    public virtual string Title { get; protected set; }

    /// <summary>
    /// 邀请码
    /// </summary>
    [Comment("邀请码")]
    [MaxLength(50)]
    public virtual string Code { get; protected set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [Comment("过期时间")]
    public virtual DateTime ExpirationTime { get; protected set; }

    /// <summary>
    /// 所属聊天对象
    /// </summary>
    [Comment("所属聊天对象")]
    public virtual long? OwnerId { get; protected set; }

    /// <summary>
    /// 所属聊天对象
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    /// <summary>
    /// 是否可用
    /// </summary>
    [Comment("是否可用")]
    public virtual bool IsEnabled { get; set; }

    public void SetCode(string code)
    {
        Code = code;
    }

    public void SetExpirationTime(DateTime expirationTime)
    {
        ExpirationTime = expirationTime;
    }

    public void SetOwnerId(long ownerId)
    {
        OwnerId = ownerId;
    }

    public void SetTitle(string title)
    {
        Title = title;
    }
}
