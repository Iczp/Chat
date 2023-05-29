using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.FavoritedRecorders.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.FavoritedRecorders
{
    public class FavoritedRecorderAppService : ChatAppService, IFavoritedRecorderAppService
    {
        protected ISessionUnitManager SessionUnitManager { get; set; }
        protected IFavoritedRecorderManager FavoritedRecorderManager { get; set; }
        protected ISessionUnitRepository SessionUnitRepository { get; set; }

        protected IRepository<FavoritedRecorder> Repository { get; set; }
        protected IChatObjectManager ChatObjectManager { get; set; }

        public FavoritedRecorderAppService(
            IFavoritedRecorderManager favoritedRecorderManager,
            ISessionUnitManager sessionUnitManager,
            ISessionUnitRepository sessionUnitRepository,
            IRepository<FavoritedRecorder> repository,
            IChatObjectManager chatObjectManager)
        {
            FavoritedRecorderManager = favoritedRecorderManager;
            SessionUnitManager = sessionUnitManager;
            SessionUnitRepository = sessionUnitRepository;
            Repository = repository;
            ChatObjectManager = chatObjectManager;
        }

        [HttpGet]
        public async Task<PagedResultDto<FavoritedRecorderDto>> GetListAsync(FavoritedRecorderGetListInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .WhereIf(input.OwnerId.HasValue, x => x.OwnerId == input.OwnerId)
                .WhereIf(input.DestinationId.HasValue, x => x.DestinationId == input.DestinationId)
                .WhereIf(input.MinSize.HasValue, x => x.Size >= input.MinSize)
                .WhereIf(input.MaxSize.HasValue, x => x.Size < input.MaxSize)
                ;
            return await GetPagedListAsync<FavoritedRecorder, FavoritedRecorderDto>(query, input);
        }

        [HttpGet]
        public Task<long> GetSizeAsync(long ownerId)
        {
            return FavoritedRecorderManager.GetSizeByOwnerIdAsync(ownerId);
        }

        [HttpGet]
        public Task<int> GetCountAsync(long ownerId)
        {
            return FavoritedRecorderManager.GetCountByOwnerIdAsync(ownerId);
        }

        [HttpPost]
        public async Task<DateTime> CreateAsync(FavoritedRecorderCreateInput input)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(input.SessionUnitId);

            var entity = await FavoritedRecorderManager.CreateIfNotContainsAsync(sessionUnit, input.MessageId, input.DeviceId);

            return entity.CreationTime;
        }

        [HttpPost]
        public Task DeleteAsync(Guid sessionUnitId, long messageId)
        {
            return FavoritedRecorderManager.DeleteAsync(sessionUnitId, messageId);
        }

       
    }
}
