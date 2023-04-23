using IczpNet.AbpTrees;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionOrganizations
{
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

    }
}
