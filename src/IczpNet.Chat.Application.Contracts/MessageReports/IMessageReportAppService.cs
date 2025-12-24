using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageReports;

public interface IMessageReportAppService
{
    Task<bool> FlushAsync(string granularity, long dateBucket);

    Task<MessageReportOptions> GetOptionsAsync();

    Task<PagedResultDto<MessageReportDto>> GetListAsync(string granularity, MessageReportGetListInput input);
}
