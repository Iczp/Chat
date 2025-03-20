using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionUnitCounters;

public interface ISessionUnitCounterManager
{
    /// <summary>
    /// increment counter
    /// </summary>
    /// <param name="args"></param>
    /// <returns>update total count</returns>
    Task<int> IncremenetAsync(SessionUnitCounterArgs args);
}
