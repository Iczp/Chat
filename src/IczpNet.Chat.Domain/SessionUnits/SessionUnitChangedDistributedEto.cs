using System;

namespace IczpNet.Chat.SessionUnits;

[Serializable]
public class SessionUnitChangedDistributedEto
{
    /// <summary>
    /// Command
    /// </summary>
    public virtual string Command { get; set; } = "changed@session-unit";

    /// <summary>
    /// Emiter's HostName
    /// </summary>
    public virtual string HostName { get; set; }

    /// <summary>
    /// 变更的会话单元
    /// </summary>
    public SessionUnitCacheItem SessionUnit { get; set; }

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"[{nameof(SessionUnitChangedDistributedEto)}]:{nameof(Command)}={Command},{nameof(HostName)}={HostName}";
    }
}
