using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization CreateInput
/// </summary>
public class SessionOrganizationCreateInput : SessionOrganizationUpdateInput
{
    public virtual Guid SessionId { get; set; }
}
