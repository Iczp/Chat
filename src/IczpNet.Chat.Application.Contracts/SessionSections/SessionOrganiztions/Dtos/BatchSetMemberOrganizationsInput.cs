using IczpNet.Chat.Enums;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;

public class BatchSetMemberOrganizationsInput
{
    /// <summary>
    /// 操作人会话单元 Id
    /// </summary>
    [Required]
    public Guid SessionUnitId { get; set; }

    /// <summary>
    /// 目标成员会话单元 Id 列表（1..200）
    /// </summary>
    [Required]
    public List<Guid> MemberSessionUnitIdList { get; set; } = new();

    /// <summary>
    /// 目标组织 Id 列表（单成员上限 10 个）
    /// </summary>
    public List<long> OrganizationIdList { get; set; } = new();

    /// <summary>
    /// 批量操作模式：Add / Remove / Replace / Clear
    /// </summary>
    public MemberOrganizationBatchMode Mode { get; set; } = MemberOrganizationBatchMode.Add;
}

public class BatchSetMemberOrganizationsResultDto
{
    public int AffectedMemberCount { get; set; }
    public List<Guid> UpdatedMemberSessionUnitIdList { get; set; } = new();
}

public class SetOrganizationRolesInput
{
    [Required]
    public Guid SessionUnitId { get; set; }

    [Required]
    public long OrganizationId { get; set; }

    public List<Guid> RoleIdList { get; set; } = new();
}

public class MemberPermissionSourceItemDto
{
    public string DefinitionId { get; set; }
    public bool IsGranted { get; set; }
    public bool FromCreator { get; set; }
    public bool FromAdminTemplate { get; set; }
    public bool FromDirectGrant { get; set; }
    public List<string> DirectRoleNames { get; set; } = new();
    public List<string> OrganizationRoleSources { get; set; } = new();
}

public class MemberPermissionSourcesDto
{
    public Guid TargetSessionUnitId { get; set; }
    public bool IsCreator { get; set; }
    public bool IsAdmin { get; set; }
    public List<SessionRoleSimpleDto> DirectRoles { get; set; } = new();
    public List<SessionOrganizationDto> Organizations { get; set; } = new();
    public List<MemberPermissionSourceItemDto> Permissions { get; set; } = new();
}
