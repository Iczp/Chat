using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.MessageReports;



public interface IMessageReportRepositoryBase<T> : IMessageReportRepository, IRepository<T, Guid> where T : MessageReportBase
{

}
