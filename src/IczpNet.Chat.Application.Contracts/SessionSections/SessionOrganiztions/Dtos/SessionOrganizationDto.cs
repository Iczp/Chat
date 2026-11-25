using IczpNet.Chat.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization Dto
/// </summary>
public class SessionOrganizationDto : SessionOrganizationInfo, IEntityDto<long>
{

    public virtual string Description { get; set; }

    /// <summary>
    /// 绑定的角色列表
    /// </summary>
    public virtual List<SessionRoleSimpleDto> BoundRoles { get; set; } = new();

    /// <summary>
    /// 直接归属该组织的成员数
    /// </summary>
    public virtual int MemberCount { get; set; }

    public virtual string GetObjectKey() => $"{Id}";
}
