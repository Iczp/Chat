using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.Bases
{
    public interface IRecorderManager<TEntity>
    {

        /// <summary>
        /// IsAnyAsync
        /// </summary>
        /// <param name="sessionUnitId"></param>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<bool> IsAnyAsync(Guid sessionUnitId,long messageId);
        /// <summary>
        /// Get Counts 
        /// </summary>
        /// <param name="messageIdList"></param>
        /// <returns></returns>
        Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);

        /// <summary>
        /// Get Count 
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<int> GetCountByMessageIdAsync(long messageId);

        /// <summary>
        /// Get Recorder MessageIdList
        /// </summary>
        /// <param name="sessionUnitId"></param>
        /// <param name="messageIdList"></param>
        /// <returns></returns>
        Task<List<long>> GetRecorderMessageIdListAsync(Guid sessionUnitId, List<long> messageIdList);

        /// <summary>
        /// 查询已读
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<IQueryable<SessionUnit>> QueryRecordedAsync(long messageId);

        /// <summary>
        /// 查询未读
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<IQueryable<SessionUnit>> QueryUnrecordedAsync(long messageId);

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="sessionUnit"></param>
        /// <param name="messageId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        Task<TEntity> CreateIfNotContainsAsync(SessionUnit sessionUnit, long messageId, string deviceId);

        /// <summary>
        /// CreateAsync Many
        /// </summary>
        /// <param name="sessionUnit"></param>
        /// <param name="messageIdList"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        Task<List<TEntity>> CreateManyAsync(SessionUnit sessionUnit, List<long> messageIdList, string deviceId);



    }
}
