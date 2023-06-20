using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization UpdateInput
/// </summary>
public class SessionOrganizationUpdateInput : BaseTreeInputDto<long>
{
    /// <summary>
    /// 说明
    /// </summary>
    public virtual string Description { get; set; }
}
