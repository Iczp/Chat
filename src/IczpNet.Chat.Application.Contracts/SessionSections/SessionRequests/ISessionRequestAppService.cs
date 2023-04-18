using IczpNet.Chat.SessionSections.SessionRequests.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionRequests;

public interface ISessionRequestAppService :
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
