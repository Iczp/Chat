using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.ScanCodes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.ScanCodes;

public interface IScanCodeAdminAppService : IApplicationService
{
    Task<PagedResultDto<ScanCodeAdminDto>> GetListAsync(
        ScanCodeAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<ScanCodeAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ScanCodeAdminDto> ScanAsync(
        ScanCodeAdminSimulateInput input);
}
