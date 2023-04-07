using IczpNet.AbpTrees;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionOrganizations
{
    public interface ISessionOrganizationAppService :
        ICrudAppService<
            SessionOrganizationDetailDto,
            SessionOrganizationDto,
            long,
            SessionOrganizationGetListInput,
            SessionOrganizationCreateInput,
            SessionOrganizationUpdateInput>
        ,
        ITreeAppService<SessionOrganizationDetailDto,
            SessionOrganizationDto,
            long,
            SessionOrganizationGetListInput,
            SessionOrganizationCreateInput,
            SessionOrganizationUpdateInput, SessionOrganizationInfo>
    {
    }
}
