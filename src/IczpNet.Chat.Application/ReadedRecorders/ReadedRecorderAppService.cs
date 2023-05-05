using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ReadedRecorders.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorderAppService : ChatAppService, IReadedRecorderAppService
    {

        protected IReadedRecorderManager ReadedRecorderManager { get; }

        public ReadedRecorderAppService(IReadedRecorderManager readedRecorderManager)
        {
            ReadedRecorderManager = readedRecorderManager;
        }

        public Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
        {
            return ReadedRecorderManager.GetCountsAsync(messageIdList);
        }

        public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(long messageId, GetListByMessageIdInput input)
        {
            var query = input.IsReaded
                ? await ReadedRecorderManager.QueryReadedAsync(messageId)
                : await ReadedRecorderManager.QueryUnreadedAsync(messageId);

            query = query.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordSessionUnitSpecification(input.Keyword).ToExpression());

            return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
        }
    }
}
