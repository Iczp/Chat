using IczpNet.Chat.Enums;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageReports;

public interface IMessageReportAppService
{
    Task<bool> FlushAsync([Required] MessageReportTypes type, long dateBucket);

    Task<MessageReportOptions> GetOptionsAsync();

    Task<PagedResultDto<MessageReportDto>> GetListAsync( MessageReportGetListInput input);

    Task<PagedResultDto<MessageSummaryDto>> GetSummaryAsync(MessageReportSummaryGetListInput input);
}
