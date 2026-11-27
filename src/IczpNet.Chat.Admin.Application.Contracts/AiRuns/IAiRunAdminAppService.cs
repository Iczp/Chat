using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.AiRuns.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.AiRuns;

public interface IAiRunAdminAppService : IApplicationService
{
    Task<PagedResultDto<AiRunAdminDto>> GetListAsync(
        AiRunAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<AiRunAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AiRunAdminDto> GetBySourceMessageIdAsync(
        long sourceMessageId);

    Task<PagedResultDto<AiRunAdminDto>> GetBySessionAsync(
        Guid sessionId,
        AiRunAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<AiRunAdminDto> CreateAsync(AiRunAdminCreateInput input);

    Task<AiRunAdminDto> RetryAsync(Guid id);

    Task<AiRunAdminDto> CancelAsync(Guid id, AiRunAdminCancelInput input = null);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteManyAsync(List<Guid> idList, CancellationToken cancellationToken = default);
}
