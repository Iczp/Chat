using IczpNet.Chat.ReadedRecorders.Dtos;
using IczpNet.Chat.SessionUnits.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ReadedRecorders
{
    public interface IReadedRecorderAppService
    {
        Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);

        Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(long messageId, GetListByMessageIdInput input);

        Task<int> SetReadedManyAsync(SetReadedManyInput input);

        Task<int> SetAllAsync(long messageId);
    }
}
