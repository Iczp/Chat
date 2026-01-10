using IczpNet.AbpCommons;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Settings;

namespace IczpNet.Chat.Follows;

public class FollowManager(
    ISessionUnitManager sessionUnitManager,
    ISessionUnitCacheManager sessionUnitCacheManager,
    IRepository<Follow> repository,
    ISettingProvider settingProvider) : DomainService, IFollowManager
{
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public ISessionUnitCacheManager SessionUnitCacheManager { get; } = sessionUnitCacheManager;
    protected IRepository<Follow> Repository { get; } = repository;
    protected ISettingProvider SettingProvider { get; } = settingProvider;

    public async Task<List<Follow>> GetFollowersAsync(Guid destinationSessionUnitId)
    {
        return [.. (await Repository.GetQueryableAsync()).Where(x => x.DestinationSessionUnitId == destinationSessionUnitId)];
    }

    public async Task<List<Guid>> GetFollowerIdListAsync(Guid destinationSessionUnitId)
    {
        return [.. (await Repository.GetQueryableAsync())
           .Where(x => x.DestinationSessionUnitId == destinationSessionUnitId)
           .Where(x => x.OwnerSessionUnitId != destinationSessionUnitId)
           .Select(x => x.OwnerSessionUnitId)];
    }

    public async Task<List<Guid>> GetFollowingIdListAsync(Guid sessionUnitId)
    {
        return [.. (await Repository.GetQueryableAsync())
          .Where(x => x.OwnerSessionUnitId == sessionUnitId)
          .Where(x => x.DestinationSessionUnitId != sessionUnitId)
          .Select(x => x.DestinationSessionUnitId)];
    }

    public async Task<int> GetFollowingCountAsync(Guid ownerId)
    {
        return await Repository.CountAsync(x => x.OwnerSessionUnitId == ownerId);
    }

    public async Task<bool> CreateAsync(Guid sessionUnitId, List<Guid> unitIdList)
    {
        var followingCount = await GetFollowingCountAsync(sessionUnitId);

        var maxFollowingCount = await SettingProvider.GetAsync<int>(ChatSettings.MaxFollowingCount);

        Assert.If(followingCount > maxFollowingCount, $"Max following count:{maxFollowingCount}");

        var owner = await SessionUnitManager.GetAsync(sessionUnitId);

        return await CreateAsync(owner, unitIdList);
    }

    public async Task<bool> CreateAsync(SessionUnit ownerUnit, List<Guid> unitIdList)
    {
        var destinationList = (await SessionUnitManager.GetManyAsync([.. unitIdList.Distinct()]))
            .Select(x => x.Value);

        Assert.If(unitIdList.Contains(ownerUnit.Id), $"Unable following oneself.");

        foreach (var item in destinationList)
        {
            Assert.If(ownerUnit.SessionId != item.SessionId, $"Not in the same session,id:{item.Id}");
        }

        var destMap = destinationList.ToDictionary(x => x.Id);

        var followedIdList = (await Repository.GetQueryableAsync())
             .Where(x => x.OwnerSessionUnitId == ownerUnit.Id)
             .Select(x => x.DestinationSessionUnitId)
             .ToList();

        var newList = unitIdList.Except(followedIdList)
            .Where(x => x != ownerUnit.Id)
            .Where(x => destMap[x] != null)
            .Select(x => new Follow(ownerUnit, destMap[x]))
            .ToList();

        if (newList.Count != 0)
        {
            await Repository.InsertManyAsync(newList, autoSave: true);
        }

        await SessionUnitCacheManager.FollowAsync(ownerUnit.OwnerId, unitIdList);
        return true;
    }

    public async Task DeleteAsync(SessionUnit ownerUnit, List<Guid> unitIdList)
    {
        await Repository.DeleteAsync(x => x.OwnerSessionUnitId == ownerUnit.Id && unitIdList.Contains(x.DestinationSessionUnitId));
        await SessionUnitCacheManager.UnfollowAsync(ownerUnit.OwnerId, unitIdList);
    }

    public async Task<int> GetFollowingCountAsync(long chatObjectId)
    {
        return await Repository.CountAsync(x => x.OwnerSessionUnit.Owner.Id == chatObjectId);
    }

    public async Task<int> GetFollowerCountAsync(Guid sessionUnitId)
    {
        return await Repository.CountAsync(x => x.DestinationSessionUnitId == sessionUnitId);
    }

    public async Task<int> GetFollowerCountAsync(long chatObjectId)
    {
        return await Repository.CountAsync(x => x.DestinationSessionUnit.Owner.Id == chatObjectId);
    }
}
