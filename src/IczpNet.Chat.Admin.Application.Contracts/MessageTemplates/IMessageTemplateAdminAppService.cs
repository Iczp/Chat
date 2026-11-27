using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.MessageTemplates.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.MessageTemplates;

public interface IMessageTemplateAdminAppService : IApplicationService
{
    Task<PagedResultDto<MessageTemplateAdminDto>> GetListAsync(
        MessageTemplateAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminDetailDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminDetailDto> CreateAsync(
        MessageTemplateAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminDetailDto> UpdateAsync(
        Guid id,
        MessageTemplateAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminPublishOutput> PublishAsync(
        Guid id,
        MessageTemplateAdminPublishInput input,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminDetailDto> SetEnabledAsync(
        Guid id,
        MessageTemplateAdminSetEnabledInput input,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<MessageTemplateAdminRevisionDto>> GetRevisionsAsync(
        Guid id,
        PagedResultRequestDto input,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminRevisionDetailDto> GetRevisionAsync(
        Guid id,
        int version,
        CancellationToken cancellationToken = default);

    Task<MessageTemplateAdminPreviewOutput> PreviewAsync(
        MessageTemplateAdminPreviewInput input,
        CancellationToken cancellationToken = default);
}
