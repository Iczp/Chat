using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.FavoriteRecorders.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.FavoriteRecorders
{
    public class FavoriteAppService : ChatAppService, IFavoriteAppService
    {
        protected ISessionUnitManager SessionUnitManager { get; set; }
        protected IFavoritedRecorderManager FavoriteManager { get; set; }
        protected ISessionUnitRepository SessionUnitRepository { get; set; }

        protected IRepository<FavoritedRecorder> Repository { get; set; }
        protected IChatObjectManager ChatObjectManager { get; set; }

        public FavoriteAppService(
            IFavoritedRecorderManager followManager,
            ISessionUnitManager sessionUnitManager,
            ISessionUnitRepository sessionUnitRepository,
            IRepository<FavoritedRecorder> repository,
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
            return await GetPagedListAsync<FavoritedRecorder, FavoriteDto>(query, input);
        }

        [HttpGet]
        public Task<long> GetSizeAsync(long ownerId)
        {
            return FavoriteManager.GetSizeByOwnerIdAsync(ownerId);
        }

        [HttpGet]
        public Task<int> GetCountAsync(long ownerId)
        {
            return FavoriteManager.GetCountByOwnerIdAsync(ownerId);
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
