using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Blobs.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.Blobs;

public interface IBlobAdminAppService : IApplicationService
{
    Task<PagedResultDto<BlobAdminDto>> GetListAsync(
        BlobAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<BlobAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<BlobAdminDto>> GetManyAsync(
        List<Guid> idList,
        CancellationToken cancellationToken = default);

    Task<BlobAdminDto> UpdateAsync(
        Guid id,
        BlobAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
