using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.SessionRequests;

public class SessionRequestAdminGetListInput : PagedResultRequestDto
{
    public string Keyword { get; set; }

    public long? OwnerId { get; set; }

    public long? DestinationId { get; set; }

    public bool? IsHandled { get; set; }

    public bool? IsAgreed { get; set; }

    public bool? IsExpired { get; set; }
}

public class SessionRequestAdminDto : EntityDto<Guid>
{
    public long OwnerId { get; set; }

    public string OwnerName { get; set; }

    public long? DestinationId { get; set; }

    public string DestinationName { get; set; }

    public string RequestMessage { get; set; }

    public bool IsHandled { get; set; }

    public bool? IsAgreed { get; set; }

    public string HandleMessage { get; set; }

    public DateTime? HandleTime { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsExpired { get; set; }

    public DateTime? ExpirationTime { get; set; }

    public DateTime CreationTime { get; set; }
}

public class SessionRequestAdminHandleInput
{
    public bool IsAgreed { get; set; }

    public string HandleMessage { get; set; }
}

public interface ISessionRequestAdminAppService : IApplicationService
{
    Task<PagedResultDto<SessionRequestAdminDto>> GetListAsync(
        SessionRequestAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<SessionRequestAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SessionRequestAdminDto> HandleAsync(
        Guid id,
        SessionRequestAdminHandleInput input,
        CancellationToken cancellationToken = default);
}
