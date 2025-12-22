using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageStats;

public interface IMessageStatRepository : IRepository<MessageStat,long>
{
    Task<long> StatAsync(Guid sessionId, MessageTypes messageType);
}
