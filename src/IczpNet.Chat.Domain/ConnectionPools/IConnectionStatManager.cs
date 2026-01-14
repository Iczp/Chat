using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.ConnectionPools;

public interface IConnectionStatManager
{

    Task RecordConnectionAsync();

    Task RecordOnlinePeakAsync(long onlineCount, string reason);

    Task<OnlinePeakInfo> GetOnlinePeakAsync();

    Task<long> SumAsync(StatGranularity granularity, DateTimeOffset start, DateTimeOffset end);
}
