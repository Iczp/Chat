using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ReadedRecorders.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorderAppService : ChatAppService, IReadedRecorderAppService
    {
        protected string SetReadedManyPolicyName { get; set; }
        protected IReadedRecorderManager ReadedRecorderManager { get; }
        protected ISessionUnitManager SessionUnitManager { get; }
        public ReadedRecorderAppService(IReadedRecorderManager readedRecorderManager, ISessionUnitManager sessionUnitManager)
        {
            ReadedRecorderManager = readedRecorderManager;
            SessionUnitManager = sessionUnitManager;
        }

        [HttpGet]
        public Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList)
        {
            return ReadedRecorderManager.GetCountsAsync(messageIdList);
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionUnitDestinationDto>> GetListByMessageIdAsync(long messageId, GetListByMessageIdInput input)
        {
            var query = input.IsReaded
                ? await ReadedRecorderManager.QueryReadedAsync(messageId)
                : await ReadedRecorderManager.QueryUnreadedAsync(messageId);

            query = query.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordSessionUnitSpecification(input.Keyword).ToExpression());

            return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
        }

        [HttpPost]
        public virtual async Task<int> SetReadedManyAsync([FromQuery] SetReadedManyInput input)
        {
            await CheckPolicyAsync(SetReadedManyPolicyName);

            var entity = await SessionUnitManager.GetAsync(input.SessunitUnitId);

            return await ReadedRecorderManager.SetReadedManyAsync(entity, input.MessageIdList, input.DeviceId);
        }
    }
}
