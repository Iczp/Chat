using IczpNet.Chat.SessionSections.SessionUnits;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IczpNet.Chat.ReadedRecorders
{
    public interface IReadedRecorderManager
    {
        Task<Dictionary<long, int>> GetCountsAsync(List<long> messageIdList);

        /// <summary>
        /// 查询已读
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<IQueryable<SessionUnit>> QueryReadedAsync(long messageId);

        /// <summary>
        /// 查询未读
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<IQueryable<SessionUnit>> QueryUnreadedAsync(long messageId);
    }
}
