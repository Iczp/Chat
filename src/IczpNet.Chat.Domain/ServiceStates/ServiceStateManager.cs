﻿using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.ServiceStates;

public class ServiceStateManager(
    IChatObjectManager chatObjectManager,
    IDistributedCache<List<ServiceStatusCacheItem>, long> cache) : DomainService, IServiceStateManager
{
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IDistributedCache<List<ServiceStatusCacheItem>, long> Cache { get; } = cache;
    protected virtual DistributedCacheEntryOptions DistributedCacheEntryOptions { get; set; } = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1),
    };

    public virtual async Task<List<ServiceStatusCacheItem>> GetAsync(long chatObjectId)
    {
        return await Cache.GetAsync(chatObjectId);
    }

    public virtual async Task<ServiceStatus?> GetStatusAsync(long chatObjectId)
    {
        var items = await GetAsync(chatObjectId);

        ServiceStatus? status = null;

        if (items != null)
        {
            status = items.Count == 1 ? items[0].Status : items.OrderByDescending(x => x.ActiveTime).FirstOrDefault().Status;
        }
        Logger.LogDebug($"ChatObjectId:{chatObjectId},ServiceStatus:{status}");

        return status;
    }

    /// <summary>
    /// 获取包含所有子账号的状态
    /// </summary>
    /// <param name="shopKeeperId"></param>
    /// <returns></returns>
    public virtual async Task<ServiceStatus?> GetAnyChildrenStatusAsync(long shopKeeperId)
    {
        var status = await GetStatusAsync(shopKeeperId);

        if (status != null)
        {
            var all = await ChatObjectManager.GetRootChildrenAsync(shopKeeperId);
            foreach (var child in all)
            {
                status = await GetStatusAsync(shopKeeperId);
                if((int)status > 0)
                {
                    break;
                }
            }
        }
        
        return null;
    }

    public virtual async Task<List<ServiceStatusCacheItem>> SetAsync(long chatObjectId, string deviceId, ServiceStatus status)
    {
        var items = await Cache.GetAsync(chatObjectId);

        if (items == null)
        {
            items = [new ServiceStatusCacheItem(chatObjectId, deviceId, status)];
        }
        else
        {
            var item = items.FirstOrDefault(x => x.DeviceId == deviceId);

            if (item != null)
            {
                item.Status = status;
                item.ActiveTime = Clock.Now;
            }
            else
            {
                items.Add(new ServiceStatusCacheItem(chatObjectId, deviceId, status));
            }
        }
        await Cache.SetAsync(chatObjectId, items, DistributedCacheEntryOptions);

        Logger.LogInformation($"ServiceStatusCacheItem Set:{items}");

        return items;
    }

    public async Task RemoveDeviceAsync(long chatObjectId, string deviceId)
    {
        var items = await Cache.GetAsync(chatObjectId);

        if (items == null)
        {
            return;
        }

        var item = items.FirstOrDefault(x => x.DeviceId == deviceId);

        if (item != null)
        {
            items.Remove(item);
        }
        if (items.Count == 0)
        {
            await Cache.RemoveAsync(chatObjectId);
            return;
        }
        await Cache.SetAsync(chatObjectId, items, DistributedCacheEntryOptions);
    }

    public async Task<Dictionary<long, List<ServiceStatusCacheItem>>> SetAppUserIdAsync(Guid appUserId, string deviceId, ServiceStatus status)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(appUserId);

        var dic = new Dictionary<long, List<ServiceStatusCacheItem>>();

        foreach (var chatObjectId in chatObjectIdList)
        {
            var item = await SetAsync(chatObjectId, deviceId, status);
            dic.TryAdd(chatObjectId, item);
        }
        return dic;
    }

    public async Task RemoveAppUserIdAsync(Guid appUserId, string deviceId)
    {
        var chatObjectIdList = await ChatObjectManager.GetIdListByUserIdAsync(appUserId);

        foreach (var chatObjectId in chatObjectIdList)
        {
            await RemoveDeviceAsync(chatObjectId, deviceId);
        }
    }

    /// <inheritdoc/>
    public Task<bool> IsIdledAsync(long chatObjectId)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<bool> IsOnlineAsync(long chatObjectId)
    {
        var status = await GetStatusAsync(chatObjectId);

        return status != null;
    }
}
