using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.Sessions;

public interface ISessionRepository : IRepository<Session, Guid>
{
    Task<int> UpdateLastMessageIdAsync(Guid sessionId, long lastMessageId);

    /// <summary>
    /// 更新当前会话的消息序号(原子递增)
    /// </summary>
    /// <param name="sessionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<long> UpdateLastMessageSeqAsync(Guid sessionId,  CancellationToken cancellationToken = default);
}
