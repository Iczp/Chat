using IczpNet.AbpCommons;
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

public class FollowManager(ISessionUnitManager sessionUnitManager,
    IRepository<Follow> repository,
    ISettingProvider settingProvider) : DomainService, IFollowManager
{
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
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

    public async Task<bool> CreateAsync(Guid sessionUnitId, List<Guid> idList)
    {
        var followingCount = await GetFollowingCountAsync(sessionUnitId);

        var maxFollowingCount = await SettingProvider.GetAsync<int>(ChatSettings.MaxFollowingCount);

        Assert.If(followingCount > maxFollowingCount, $"Max following count:{maxFollowingCount}");

        var owner = await SessionUnitManager.GetAsync(sessionUnitId);

        return await CreateAsync(owner, idList);
    }

    public async Task<bool> CreateAsync(SessionUnit owner, List<Guid> idList)
    {
        var destinationList = (await SessionUnitManager.GetManyAsync([.. idList.Distinct()]))
            .Select(x => x.Value);

        Assert.If(idList.Contains(owner.Id), $"Unable following oneself.");

        foreach (var item in destinationList)
        {
            Assert.If(owner.SessionId != item.SessionId, $"Not in the same session,id:{item.Id}");
        }

        var followedIdList = (await Repository.GetQueryableAsync())
             .Where(x => x.OwnerSessionUnitId == owner.Id)
             .Select(x => x.DestinationSessionUnitId)
             .ToList();

        var newList = idList.Except(followedIdList)
            .Where(x => x != owner.Id)
            .Select(x => new Follow(owner, x))
            .ToList();

        if (newList.Count != 0)
        {
            await Repository.InsertManyAsync(newList, autoSave: true);
        }

        return true;
    }

    public async Task DeleteAsync(Guid sessionUnitId, List<Guid> idList)
    {
        await Repository.DeleteAsync(x => x.OwnerSessionUnitId == sessionUnitId && idList.Contains(x.DestinationSessionUnitId));
    }

    public async Task DeleteAsync(SessionUnit owner, List<Guid> idList)
    {
        await DeleteAsync(owner.Id, idList);
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
