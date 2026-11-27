using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IczpNet.Chat.Admin.Online.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Admin.Online;

public interface IOnlineAdminAppService : IApplicationService
{
    Task<PagedResultDto<OnlineAdminConnectionDto>> GetListAsync(OnlineAdminGetListInput input);

    Task<OnlineAdminConnectionDto> GetAsync(string id);

    Task<long> GetTotalCountAsync(string host = null);

    Task<PagedResultDto<OnlineAdminHostDto>> GetHostsAsync(OnlineAdminHostGetListInput input);

    Task<PagedResultDto<OnlineAdminConnectionDto>> GetByUserAsync(Guid userId, PagedAndSortedResultRequestDto input);

    Task<PagedResultDto<OnlineAdminConnectionDto>> GetByOwnerAsync(long ownerId, PagedAndSortedResultRequestDto input);

    Task<long> GetCountByUserAsync(Guid userId);

    Task<long> GetCountByOwnerAsync(long ownerId);

    Task<long> GetCountBySessionAsync(Guid sessionId);

    Task<PagedResultDto<OnlineAdminLastOnlineDto>> GetLastOnlineAsync(long ownerId, OnlineAdminLastOnlineGetListInput input);

    Task<long> GetOnlineFriendsCountAsync(long ownerId);

    Task AbortAsync(OnlineAdminAbortInput input);

    Task DeleteAsync(string connectionId);

    Task<Dictionary<string, long>> ClearAllAsync(List<string> hosts = null, string reason = null);
}
