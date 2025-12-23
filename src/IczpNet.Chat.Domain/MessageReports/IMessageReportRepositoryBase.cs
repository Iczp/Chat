using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageReports;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageStats;

public interface IMessageReportRepositoryBase<T> : IRepository<T, Guid> where T : MessageReportBase
{
    Task IncrementAsync(Guid sessionId, MessageTypes messageType, string dateBucketFormat = "yyyyMMdd");
}
