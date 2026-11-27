using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ArticleMessageSchedules.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.ArticleMessageSchedules;

public interface IArticleMessageScheduleAdminAppService : IApplicationService
{
    Task<ArticleMessageScheduleAdminDto> CreateAsync(
        ArticleMessageScheduleAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<ArticleMessageScheduleAdminDto>> GetListAsync(
        ArticleMessageScheduleAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<ArticleMessageScheduleAdminDto> RescheduleAsync(
        Guid id,
        ArticleMessageScheduleAdminTimeInput input,
        CancellationToken cancellationToken = default);

    Task<ArticleMessageScheduleAdminDto> CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ArticleMessageScheduleAdminDto> ExecuteNowAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
