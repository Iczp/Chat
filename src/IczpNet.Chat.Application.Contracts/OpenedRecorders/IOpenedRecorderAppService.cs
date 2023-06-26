using IczpNet.Chat.OpenedRecorders.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.OpenedRecorders
{
    public interface IOpenedRecorderAppService
    {
        Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);

        Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(GetListByMessageIdInput input);

        Task<OpenedRecorderDto> SetOpenedAsync(OpenedRecorderInput input);
    }
}
