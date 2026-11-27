using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.SessionUnits.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.SessionUnits;

public interface ISessionUnitAdminAppService : IApplicationService
{
    Task<PagedResultDto<SessionUnitAdminDto>> GetListAsync(
        SessionUnitAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<SessionUnitAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> FindIdAsync(
        [Required] long ownerId,
        [Required] long destinactionId,
        CancellationToken cancellationToken = default);
}
