using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Follows;

/// <summary>
/// 关注
/// </summary>
public class Follow : BaseEntity
{
    /// <summary>
    /// ChatObjectId
    /// </summary>
    public virtual long? OwnerId { get; protected set; }

    /// <summary>
    /// ChatObject
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public virtual ChatObject Owner { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectTypeEnums? OwnerType { get; protected set; }

    /// <summary>
    /// ChatObjectId
    /// </summary>
    public virtual long? DestinationId { get; protected set; }

    /// <summary>
    /// Target ChatObject
    /// </summary>
    [ForeignKey(nameof(DestinationId))]
    public virtual ChatObject Destination { get; protected set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationType { get; protected set; }

    /// <summary>
    /// Owner SessionUnitId
    /// </summary>
    public virtual Guid OwnerSessionUnitId { get; protected set; }

    /// <summary>
    /// SessionUnit
    /// </summary>
    [ForeignKey(nameof(OwnerSessionUnitId))]
    public virtual SessionUnit OwnerSessionUnit { get; protected set; }

    /// <summary>
    /// Destination SessionUnitId
    /// </summary>
    public virtual Guid DestinationSessionUnitId { get; protected set; }

    //[ForeignKey(nameof(OwnerSessionUnitId))]
    //public virtual SessionUnit DestinationSessionUnit { get; set; }

    public override object[] GetKeys()
    {
        return [OwnerSessionUnitId, DestinationSessionUnitId];
    }

    protected Follow() { }

    public Follow(SessionUnit ownerSessionUnit, Guid destinationId)
    {
        OwnerSessionUnit = ownerSessionUnit; 
        DestinationSessionUnitId = destinationId;
    }
}
