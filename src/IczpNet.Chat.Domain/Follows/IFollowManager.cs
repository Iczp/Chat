using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using IczpNet.Chat.SessionUnits;

namespace IczpNet.Chat.Follows
{
    public interface IFollowManager
    {

        Task<List<Follow>> GetFollowersAsync(Guid destinationSessionUnitId);

        Task<List<Guid>> GetFollowerIdListAsync(Guid destinationSessionUnitId);

        Task<List<Guid>> GetFollowingIdListAsync(Guid sessionUnitId);

        Task<int> GetFollowingCountAsync(Guid sessionUnitId);

        Task<bool> CreateAsync(Guid sessionUnitId, List<Guid> idList);

        Task<bool> CreateAsync(SessionUnit owner, List<Guid> idList);

        Task DeleteAsync(Guid sessionUnitId, List<Guid> idList);

        Task DeleteAsync(SessionUnit owner, List<Guid> idList);
        
    }
}
