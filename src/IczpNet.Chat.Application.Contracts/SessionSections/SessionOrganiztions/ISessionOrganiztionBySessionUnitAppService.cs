using IczpNet.AbpTrees;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using IczpNet.Chat.SessionSections.SessionRoles.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions;

public interface ISessionOrganiztionBySessionUnitAppService :
    ICrudAppService<
        SessionOrganizationDetailDto,
        SessionOrganizationDto,
        long,
        SessionOrganizationGetListBySessionUnitInput,
        SessionOrganizationCreateBySessionUnitInput,
        SessionOrganizationUpdateInput>
    ,
    ITreeAppService<SessionOrganizationDetailDto,
        SessionOrganizationDto,
        long,
        SessionOrganizationGetListBySessionUnitInput,
        SessionOrganizationCreateBySessionUnitInput,
        SessionOrganizationUpdateInput>
{
    Task<BatchSetMemberOrganizationsResultDto> BatchSetMembersAsync(BatchSetMemberOrganizationsInput input);

    Task<List<SessionRoleSimpleDto>> GetOrganizationRolesAsync(Guid sessionUnitId, long organizationId);

    Task<List<SessionRoleSimpleDto>> SetOrganizationRolesAsync(SetOrganizationRolesInput input);

    Task<MemberPermissionSourcesDto> GetMemberPermissionSourcesAsync(Guid sessionUnitId, Guid targetSessionUnitId);
}
