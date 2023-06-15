using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
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
        public ReadedRecorderAppService(
            IReadedRecorderManager readedRecorderManager)
        {
            ReadedRecorderManager = readedRecorderManager;
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
                ? await ReadedRecorderManager.QueryRecordedAsync(messageId)
                : await ReadedRecorderManager.QueryUnrecordedAsync(messageId);

            query = query.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)));

            return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query, input);
        }

        [HttpPost]
        public virtual async Task<int> SetReadedManyAsync([FromQuery] SetReadedManyInput input)
        {
            await CheckPolicyAsync(SetReadedManyPolicyName);

            var sessionUnit = await SessionUnitManager.GetAsync(input.SessunitUnitId);

            var entities = await ReadedRecorderManager.CreateManyAsync(sessionUnit, input.MessageIdList, input.DeviceId);

            return entities.Count;
        }

        [HttpPost]
        public Task<int> SetAllAsync(long messageId)
        {
            return ReadedRecorderManager.CreateAllAsync(messageId);
        }
    }
}
