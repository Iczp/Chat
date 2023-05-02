﻿using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.ShopKeepers.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ShopKeepers
{
    public class ShopKeeperAppService : ChatAppService, IShopKeeperAppService
    {
        protected IChatObjectRepository Repository { get; }
        protected IShopKeeperManager ShopKeeperManager { get; }

        public ShopKeeperAppService(IChatObjectRepository repository, 
            IShopKeeperManager shopKeeperManager)
        {
            Repository = repository;
            ShopKeeperManager = shopKeeperManager;
        }

        [HttpGet]
        public virtual async Task<PagedResultDto<ShopKeeperDto>> GetListAsync(ShopKeeperGetListInput input)
        {
            var query = (await Repository.GetQueryableAsync())
                .Where(x => x.ObjectType == ChatObjectTypeEnums.ShopKeeper);

            return await GetPagedListAsync<ChatObject, ShopKeeperDto>(query, input, x => x.OrderBy(d => d.Name));
        }

        [HttpPost]
        public Task<PagedResultDto<ShopKeeperDto>> CreateAsync(ShopKeeperCreateInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}