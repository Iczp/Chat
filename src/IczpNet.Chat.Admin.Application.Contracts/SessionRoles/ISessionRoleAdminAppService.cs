using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.SessionRoles;

public class SessionRoleAdminGetListInput : PagedResultRequestDto
{
    public Guid? SessionId { get; set; }

    public string Keyword { get; set; }

    public bool? IsDefault { get; set; }
}

public class SessionRoleAdminDto : EntityDto<Guid>
{
    public Guid? SessionId { get; set; }

    public string SessionTitle { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsDefault { get; set; }

    public int PermissionCount { get; set; }

    public DateTime CreationTime { get; set; }
}

public interface ISessionRoleAdminAppService : IApplicationService
{
    Task<PagedResultDto<SessionRoleAdminDto>> GetListAsync(
        SessionRoleAdminGetListInput input,
        CancellationToken cancellationToken = default);

    Task<SessionRoleAdminDto> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
