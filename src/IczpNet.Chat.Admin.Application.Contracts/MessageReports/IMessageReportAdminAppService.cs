using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.MessageReports.Dtos;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.MessageReports;

public interface IMessageReportAdminAppService : IApplicationService
{
    Task<PagedResultDto<MessageReportAdminDto>> GetListAsync(
        MessageReportAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<MessageReportAdminSummaryDto>> GetSummaryAsync(
        MessageReportAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<MessageReportAdminOptionsDto> GetOptionsAsync();

    Task<bool> FlushAsync(
        [Required] MessageReportTypes type,
        long? dateBucket = null);
}
