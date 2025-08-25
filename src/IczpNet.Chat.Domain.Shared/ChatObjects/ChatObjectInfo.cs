using IczpNet.AbpTrees;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectInfo : TreeInfo<long>, IChatObject
{
    /// <summary>
    /// 
    /// </summary>
    public virtual string DisplayName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string ChatObjectTypeId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string Code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Genders Gender { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string Thumbnail { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string Portrait { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Guid? AppUserId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsPublic { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsEnabled { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsDefault { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectTypeEnums? ObjectType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string ObjectTypeDescription { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ServiceStatus? ServiceStatus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string ServiceStatusDescription { get; set; }
}
