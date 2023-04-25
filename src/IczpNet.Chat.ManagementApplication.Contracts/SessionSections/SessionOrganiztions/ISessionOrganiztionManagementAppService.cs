using IczpNet.AbpTrees;
using IczpNet.Chat.Management.SessionSections.SessionOrganiztions.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.Management.SessionSections.SessionOrganizations
{
    public interface ISessionOrganizationManagementAppService :
        ICrudAppService<
            SessionOrganizationDetailManagementDto,
            SessionOrganizationManagementDto,
            long,
            SessionOrganizationGetListManagementInput,
            SessionOrganizationCreateManagementInput,
            SessionOrganizationUpdateManagementInput>
        ,
        ITreeAppService<SessionOrganizationDetailManagementDto,
            SessionOrganizationManagementDto,
            long,
            SessionOrganizationGetListManagementInput,
            SessionOrganizationCreateManagementInput,
            SessionOrganizationUpdateManagementInput>
    {

    }
}
