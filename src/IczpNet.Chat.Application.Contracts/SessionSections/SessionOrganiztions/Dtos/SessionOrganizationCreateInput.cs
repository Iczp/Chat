using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

public class SessionOrganizationCreateInput : SessionOrganizationCreateBySessionUnitInput, ISessionId
{
    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }
}
