using IczpNet.Chat.EntityFrameworkCore;
using IczpNet.Chat.MessageReports;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.MessageStats;

public class MessageReportDayRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : MessageReportRepositoryBase<MessageReportDay>(dbContextProvider), IMessageReportDayRepository
{
}
