using IczpNet.Chat.Management.SessionSections.SessionRequests.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.SessionSections.SessionRequests;

public interface ISessionRequestManagementAppService :
    ICrudAppService<
        SessionRequestDetailDto,
        SessionRequestDto,
        Guid,
        SessionRequestGetListInput,
        SessionRequestCreateInput,
        SessionRequestUpdateInput>
{
    Task<SessionRequestDetailDto> HandleRequestAsync(SessionRequestHandleInput input);
}
