using IczpNet.Chat.Management.BaseDtos;
using System;

namespace IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization UpdateInput
/// </summary>
public class SessionOrganizationUpdateManagementInput : BaseTreeInputDto<long>
{
    public virtual string Description { get; set; }
}
