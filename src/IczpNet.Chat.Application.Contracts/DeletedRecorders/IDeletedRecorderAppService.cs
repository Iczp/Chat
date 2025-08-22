using IczpNet.Chat.DeletedRecorders.Dtos;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.DeletedRecorders;

public interface IDeletedRecorderAppService
{
    Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);

    Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(GetListByMessageIdInput input);

    Task<DeletedRecorderDto> SetDeletedAsync(DeletedRecorderInput input);
}
