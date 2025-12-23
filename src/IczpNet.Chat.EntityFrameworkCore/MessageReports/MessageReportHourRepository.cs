using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.MessageReports;

public class MessageReportHourRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : MessageReportRepositoryBase<MessageReportHour>(dbContextProvider), IMessageReportHourRepository
{
}
