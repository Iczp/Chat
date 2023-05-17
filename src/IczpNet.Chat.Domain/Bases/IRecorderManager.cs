using IczpNet.Chat.SessionSections.SessionUnits;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.Bases
{
    public interface IRecorderManager<TEntity>
    {
        Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);

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

        Task<List<TEntity>> CreateManyAsync(SessionUnit sessionUnit, List<long> messageIdList, string deviceId);
    }
}
