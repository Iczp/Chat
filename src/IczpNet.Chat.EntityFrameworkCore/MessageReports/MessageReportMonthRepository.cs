using IczpNet.Chat.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace IczpNet.Chat.MessageReports;

public class MessageReportMonthRepository(IDbContextProvider<ChatDbContext> dbContextProvider) : MessageReportRepositoryBase<MessageReportMonth>(dbContextProvider), IMessageReportMonthRepository
{
}
