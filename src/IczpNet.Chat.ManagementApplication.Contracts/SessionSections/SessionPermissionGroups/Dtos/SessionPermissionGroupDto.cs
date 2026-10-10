using IczpNet.Chat.SessionPermissionGroups;
using System;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;

/// <summary>
/// SessionPermissionGroup Dto
/// </summary>
public class SessionPermissionGroupDto : SessionPermissionGroupInfo, IEntityDto<long>
{

    public virtual string Description { get; set; }

    public virtual string GetObjectKey() => $"{Id}";
}
