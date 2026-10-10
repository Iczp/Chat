using IczpNet.Chat.Sessions;
using System;

namespace IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization GetListInput
/// </summary>
public class SessionOrganizationGetListManagementInput : SessionOrganizationGetListBySessionUnitInput, ISessionId
{
    public virtual Guid? SessionId { get; set; }
}
