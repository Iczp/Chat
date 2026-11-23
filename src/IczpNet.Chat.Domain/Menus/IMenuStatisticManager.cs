using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Menus;

public interface IMenuStatisticManager
{
    /// <summary>
    /// 记录菜单点击事件（原子累加 PV，HyperLogLog 记录 UV）
    /// </summary>
    Task RecordClickAsync(Guid menuId, long chatObjectId, long? senderId = null);

    /// <summary>
    /// 获取单个菜单的点击统计（总 PV、总 UV、今日 PV、今日 UV）
    /// </summary>
    Task<(long TotalClicks, long UniqueUsers, long TodayClicks, long TodayUsers)> GetMenuStatsAsync(Guid menuId);

    /// <summary>
    /// 批量获取多个菜单的统计数据
    /// </summary>
    Task<Dictionary<Guid, (long TotalClicks, long UniqueUsers, long TodayClicks, long TodayUsers)>> GetBatchMenuStatsAsync(IEnumerable<Guid> menuIds);

    /// <summary>
    /// 将 Redis 中的脏统计增量落库到数据库（Write-Behind 异步刷盘）
    /// </summary>
    Task<int> FlushDirtyAsync();
}
