using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.EntryNames.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.EntryNames;

public interface IEntryNameAdminAppService : IApplicationService
{
    Task<PagedResultDto<EntryNameAdminDto>> GetListAsync(
        EntryNameAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<EntryNameAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EntryNameAdminDto> CreateAsync(
        EntryNameAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<EntryNameAdminDto> UpdateAsync(
        Guid id,
        EntryNameAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
