using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using IczpNet.Chat.SessionUnits;

namespace IczpNet.Chat.Follows;

public interface IFollowManager
{

    Task<List<Follow>> GetFollowersAsync(Guid destinationSessionUnitId);

    Task<List<Guid>> GetFollowerIdListAsync(Guid destinationSessionUnitId);

    Task<List<Guid>> GetFollowingIdListAsync(Guid sessionUnitId);

    Task<int> GetFollowingCountAsync(Guid sessionUnitId);

    Task<int> GetFollowingCountAsync(long chatObjectId);

    Task<int> GetFollowerCountAsync(Guid sessionUnitId);

    Task<int> GetFollowerCountAsync(long chatObjectId);

    Task<bool> CreateAsync(Guid sessionUnitId, List<Guid> unitIdList);

    Task<bool> CreateAsync(SessionUnit ownerUnit, List<Guid> unitIdList);

    //Task DeleteAsync(Guid sessionUnitId, List<Guid> idList);

    Task DeleteAsync(SessionUnit ownerUnit, List<Guid> unitIdList);
    
}
