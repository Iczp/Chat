using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization CreateInput
/// </summary>
public class SessionOrganizationCreateInput : SessionOrganizationUpdateInput, ISessionId
{
    public virtual Guid? SessionId { get; set; }
}
