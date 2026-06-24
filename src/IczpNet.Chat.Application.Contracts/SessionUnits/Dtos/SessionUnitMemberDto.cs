using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitMemberDto : SessionUnitMemberSenderDto
{
    /// <summary>
    /// DestinationId
    /// </summary>
    public virtual long? DestinationId { get; set; }

    /// <summary>
    /// DestinationType
    /// </summary>
    public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }


    /// <summary>
    /// Ticks
    /// </summary>
    public virtual double Ticks { get; set; }

    /// <summary>
    /// Sorting
    /// </summary>
    public virtual double Sorting { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual DateTime CreationTime { get; set; }

    /// <summary>
    /// Member Scrore
    /// </summary>
    public virtual double? Score { get; set; }

}
