using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization GetListInput
/// </summary>
public class SessionOrganizationGetListInput : BaseTreeGetListInput<long>, ISessionId
{
    public virtual Guid? SessionId { get; set; }
}
