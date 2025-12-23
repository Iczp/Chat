using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageStats;

public interface IMessageStatRepository : IRepository<MessageStat, Guid>
{
    Task IncrementAsync(Guid sessionId, MessageTypes messageType, string dateBucketFormat = "yyyyMMdd");
}
