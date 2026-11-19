using System;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话处置管理器
/// </summary>
public interface ISessionUnitDisposalManager
{
    /// <summary>
    /// 记录一次会话处置行为并推送实时事件
    /// </summary>
    Task<SessionUnitDisposalRecord> RecordDisposalAsync(
        long ownerId,
        Guid sessionUnitId,
        Guid? sessionId,
        long? destinationId,
        SessionUnitDisposalType disposalType,
        SessionUnitDisposalReason reason,
        long? operatorId = null,
        string description = null);
}
