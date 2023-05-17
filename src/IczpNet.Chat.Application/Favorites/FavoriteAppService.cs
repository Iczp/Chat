using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Favorites.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Favorites
{
    public class FavoriteAppService : ChatAppService, IFavoriteAppService
    {
        protected ISessionUnitManager SessionUnitManager { get; set; }
        protected IFavoriteManager FavoriteManager { get; set; }
        protected ISessionUnitRepository SessionUnitRepository { get; set; }

        protected IRepository<Favorite> Repository { get; set; }
        protected IChatObjectManager ChatObjectManager { get; set; }

        public FavoriteAppService(
            IFavoriteManager followManager,
            ISessionUnitManager sessionUnitManager,
            ISessionUnitRepository sessionUnitRepository,
            IRepository<Favorite> repository,
            IChatObjectManager chatObjectManager)
        {
            FavoriteManager = followManager;
            SessionUnitManager = sessionUnitManager;
            SessionUnitRepository = sessionUnitRepository;
            Repository = repository;
            ChatObjectManager = chatObjectManager;
        }

        [HttpGet]
        public async Task<PagedResultDto<FavoriteDto>> GetListAsync(FavoriteGetListInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
                .WhereIf(input.MinSize.HasValue, x => x.Size >= input.MinSize)
                .WhereIf(input.MaxSize.HasValue, x => x.Size < input.MaxSize)
                ;
            return await GetPagedListAsync<Favorite, FavoriteDto>(query, input);
        }

        [HttpPost]
        public async Task<DateTime> CreateAsync(FavoriteCreateInput input)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);

            var entity = await FavoriteManager.CreateIfNotContainsAsync(sessionUnit, input.MessageId, input.DeviceId);

            return entity.CreationTime;
        }

        [HttpPost]
        public Task DeleteAsync(Guid sessionUnitId, long messageId)
        {
            return FavoriteManager.DeleteAsync(sessionUnitId, messageId);
        }
    }
}
