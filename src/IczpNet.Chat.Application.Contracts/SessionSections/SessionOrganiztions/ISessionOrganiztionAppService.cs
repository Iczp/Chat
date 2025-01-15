using IczpNet.AbpTrees;
using IczpNet.Chat.SessionSections.SessionOrganizations;
using IczpNet.Chat.SessionSections.SessionOrganiztions.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionOrganiztions;

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
