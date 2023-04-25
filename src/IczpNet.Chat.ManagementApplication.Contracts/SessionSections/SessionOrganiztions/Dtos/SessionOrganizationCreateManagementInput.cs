using IczpNet.Chat.SessionSections;
using System;

namespace IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization CreateInput
/// </summary>
public class SessionOrganizationCreateManagementInput : SessionOrganizationCreateBySessionUnitManagementInput, ISessionId
{
    public virtual Guid? SessionId { get; set; }
}
