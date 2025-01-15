using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Developers;

/// <summary>
/// 开发者
/// </summary>
[Comment("开发者")]
public class Developer : BaseEntity, IChatOwner<long>, IIsEnabled
{
    public virtual long OwnerId { get; set; }

    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; set; }

    /// <summary>
    /// 开发者设置的Token
    /// </summary>
    [StringLength(50)]
    [Comment("Token")] 
    public virtual string Token { get;  set; }

    /// <summary>
    /// 开发者设置的EncodingAESKey
    /// </summary>
    [StringLength(43)]
    [Comment("EncodingAesKey")]
    public virtual string EncodingAesKey { get;  set; }

    /// <summary>
    /// 开发者设置的Url
    /// </summary>
    [StringLength(256)]
    [Comment("开发者设置的Url")]
    public virtual string PostUrl { get; set; }

    /// <summary>
    /// 是否启用开发者
    /// </summary>
    [Comment("是否启用开发者")] 
    public virtual bool IsEnabled { get; set; }

    ///// <summary>
    ///// 是否验证
    ///// </summary>
    //public virtual bool IsVerified { get; set; }

    public override object[] GetKeys()
    {
        return new object[] { OwnerId };
    }
}
