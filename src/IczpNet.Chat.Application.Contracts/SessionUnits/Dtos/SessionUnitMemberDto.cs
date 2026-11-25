using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using System;
using System.Collections.Generic;

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

    /// <summary>
    /// 所属角色列表
    /// </summary>
    public virtual List<SessionRoleSimpleDto> RoleList { get; set; }

    /// <summary>
    /// 所属组织列表
    /// </summary>
    public virtual List<SessionOrganizationDto> OrganizationList { get; set; }

}
