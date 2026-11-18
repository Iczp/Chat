using IczpNet.Chat.SessionOrganizations;
using System;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization Dto
/// </summary>
public class SessionOrganizationManagementDto : SessionOrganizationInfo, IEntityDto<long>
{

    public virtual string Description { get; set; }

    public virtual string GetObjectKey() => $"{Id}";
}
