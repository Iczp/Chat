using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.EntryValues.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.EntryValues;

public interface IEntryValueAdminAppService : IApplicationService
{
    Task<PagedResultDto<EntryValueAdminDto>> GetListAsync(
        EntryValueAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<EntryValueAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EntryValueAdminDto> CreateAsync(
        EntryValueAdminCreateInput input,
        CancellationToken cancellationToken = default);

    Task<EntryValueAdminDto> UpdateAsync(
        Guid id,
        EntryValueAdminUpdateInput input,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
