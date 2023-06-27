using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization GetListInput
/// </summary>
public class SessionOrganizationGetListInput : SessionOrganizationGetListBySessionUnitInput//, ISessionId
{
    /// <summary>
    /// 会话Id
    /// </summary>
    public virtual Guid? SessionId { get; set; }

    /// <summary>
    /// 会话单元Id
    /// </summary>
    public virtual Guid? SessionUnitId { get; set; }
}
