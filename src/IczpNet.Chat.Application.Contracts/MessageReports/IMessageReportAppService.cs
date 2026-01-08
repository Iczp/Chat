using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageReports;

public interface IMessageReportAppService
{
    Task<bool> FlushAsync([Required] string granularity, long dateBucket);

    Task<MessageReportOptions> GetOptionsAsync();

    Task<PagedResultDto<MessageReportDto>> GetListAsync([Required] string granularity, MessageReportGetListInput input);

    Task<PagedResultDto<MessageSummaryDto>> GetSummaryAsync([Required] string granularity, MessageReportSummaryGetListInput input);
}
