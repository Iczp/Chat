using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Follows.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Follows
{
    public class FollowAppService : ChatAppService, IFollowAppService
    {

        protected ISessionUnitManager SessionUnitManager { get; set; }
        protected IFollowManager FollowManager { get; set; }
        protected ISessionUnitRepository SessionUnitRepository { get; set; }

        protected IRepository<Follow> Repository { get; set; }
        protected IChatObjectManager ChatObjectManager { get; set; }

        public FollowAppService(
            IFollowManager followManager,
            ISessionUnitManager sessionUnitManager,
            ISessionUnitRepository sessionUnitRepository,
            IRepository<Follow> repository,
            IChatObjectManager chatObjectManager)
        {
            FollowManager = followManager;
            SessionUnitManager = sessionUnitManager;
            SessionUnitRepository = sessionUnitRepository;
            Repository = repository;
            ChatObjectManager = chatObjectManager;
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionUnitDestinationDto>> GetFollowingsAsync(GetFollowingsInput input)
        {
            var owner = await SessionUnitManager.GetAsync(input.OwnerId);

            var ownerSessionUnitIdList = (await Repository.GetQueryableAsync()).Where(x => x.OwnerId == owner.Id).Select(x => x.DestinationId);

            var query = (await SessionUnitRepository.GetQueryableAsync())
                .Where(x => ownerSessionUnitIdList.Contains(x.Id))
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordOwnerSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)))
                ;

            return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query);
        }

        [HttpGet]
        public async Task<PagedResultDto<SessionUnitDestinationDto>> GetFollowersAsync(GetFollowersInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.DestinationId == input.DestinationId)
                .Select(x => x.Owner)
                //.WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword).ToExpression())
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), new KeywordDestinationSessionUnitSpecification(input.Keyword, await ChatObjectManager.QueryByKeywordAsync(input.Keyword)))
                ;

            return await GetPagedListAsync<SessionUnit, SessionUnitDestinationDto>(query);
        }

        [HttpPost]
        public async Task<bool> CreateAsync(FollowCreateInput input)
        {
            var owner = await SessionUnitManager.GetAsync(input.OwnerId);
            //check owner

            return await FollowManager.CreateAsync(owner, input.IdList);
        }

        [HttpPost]
        public async Task DeleteAsync(Guid ownerId, List<Guid> idList)
        {
            var owner = await SessionUnitManager.GetAsync(ownerId);
            //check owner
            await FollowManager.DeleteAsync(owner, idList);
        }


    }
}
