using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization GetListInput
/// </summary>
public class SessionOrganizationGetListInput : SessionOrganizationGetListBySessionUnitInput, ISessionId
{
    public virtual Guid? SessionId { get; set; }
}
