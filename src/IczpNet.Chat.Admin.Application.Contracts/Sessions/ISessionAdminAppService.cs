using System;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.Sessions;

public class SessionAdminGetListInput : PagedResultRequestDto
{
    public string Keyword { get; set; }

    public long? OwnerId { get; set; }

    public Channels? Channel { get; set; }

    public bool? IsEnabled { get; set; }

    public bool? IsAllMuted { get; set; }
}

public class SessionAdminSetIsEnabledInput
{
    public bool IsEnabled { get; set; }
}

public class SessionAdminSetIsAllMutedInput
{
    public bool IsAllMuted { get; set; }
}

public class SessionAdminDto : EntityDto<Guid>
{
    public string SessionKey { get; set; }

    public Channels Channel { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public long? OwnerId { get; set; }

    public string OwnerName { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsAllMuted { get; set; }

    public long? FirstMessageId { get; set; }

    public long? LastMessageId { get; set; }

    public long LastMessageSeq { get; set; }

    public int MessageTotalCount { get; set; }

    public DateTime CreationTime { get; set; }
}

public interface ISessionAdminAppService : IApplicationService
{
    Task<PagedResultDto<SessionAdminDto>> GetListAsync(
        SessionAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<SessionAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SessionAdminDto> SetIsEnabledAsync(
        Guid id,
        SessionAdminSetIsEnabledInput input,
        CancellationToken cancellationToken = default);

    Task<SessionAdminDto> SetIsAllMutedAsync(
        Guid id,
        SessionAdminSetIsAllMutedInput input,
        CancellationToken cancellationToken = default);
}
