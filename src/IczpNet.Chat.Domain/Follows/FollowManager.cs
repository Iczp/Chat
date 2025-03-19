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
        return [.. (await Repository.GetQueryableAsync()).Where(x => x.DestinationId == destinationSessionUnitId)];
    }

    public async Task<List<Guid>> GetFollowerIdListAsync(Guid destinationSessionUnitId)
    {
        return [.. (await Repository.GetQueryableAsync())
           .Where(x => x.DestinationId == destinationSessionUnitId)
           .Where(x => x.SessionUnitId != destinationSessionUnitId)
           .Select(x => x.SessionUnitId)];
    }

    public async Task<List<Guid>> GetFollowingIdListAsync(Guid sessionUnitId)
    {
        return [.. (await Repository.GetQueryableAsync())
          .Where(x => x.SessionUnitId == sessionUnitId)
          .Where(x => x.DestinationId != sessionUnitId)
          .Select(x => x.DestinationId)];
    }

    public async Task<int> GetFollowingCountAsync(Guid ownerId)
    {
        return await Repository.CountAsync(x => x.SessionUnitId == ownerId);
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
        var destinationList = await SessionUnitManager.GetManyAsync(idList.Distinct().ToList());

        Assert.If(idList.Contains(owner.Id), $"Unable following oneself.");

        foreach (var item in destinationList)
        {
            Assert.If(owner.SessionId != item.SessionId, $"Not in the same session,id:{item.Id}");
        }

        var followedIdList = (await Repository.GetQueryableAsync())
             .Where(x => x.SessionUnitId == owner.Id)
             .Select(x => x.DestinationId)
             .ToList();

        var newList = idList.Except(followedIdList)
            .Where(X => X != owner.Id)
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
        await Repository.DeleteAsync(x => x.SessionUnitId == sessionUnitId && idList.Contains(x.DestinationId));
    }

    public async Task DeleteAsync(SessionUnit owner, List<Guid> idList)
    {
        await DeleteAsync(owner.Id, idList);
    }


}
