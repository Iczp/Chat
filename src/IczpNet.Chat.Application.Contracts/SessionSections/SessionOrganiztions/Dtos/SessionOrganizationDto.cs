using IczpNet.Chat.SessionSections.SessionOrganizations;
using System;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionOrganization Dto
/// </summary>
public class SessionOrganizationDto : SessionOrganizationInfo, IEntityDto<long>
{

    public virtual string Description { get; set; }
}
