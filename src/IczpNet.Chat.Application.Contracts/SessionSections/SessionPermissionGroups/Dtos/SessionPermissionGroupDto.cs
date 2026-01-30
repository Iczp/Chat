using IczpNet.Chat.SessionPermissionGroups;
using System;
using Volo.Abp.Application.Dtos;
namespace IczpNet.Chat.SessionSections.SessionPermissionGroups.Dtos;

/// <summary>
/// SessionPermissionGroup Dto
/// </summary>
public class SessionPermissionGroupDto : SessionPermissionGroupInfo, IEntityDto<long>
{

    public virtual string Description { get; set; }
}
