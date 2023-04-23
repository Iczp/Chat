using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization UpdateInput
/// </summary>
public class SessionOrganizationUpdateInput : BaseTreeInputDto<long>
{
    public virtual string Description { get; set; }
}
