using IczpNet.Chat.Locations.Dto;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Locations;

public interface ILocationAppService : IApplicationService
{
    /// <summary>
    /// 共享位置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<ShareLocationOutput> SharingAsync(ShareLocationInput input);

    /// <summary>
    /// 获取媒体位置信息
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<PagedResultDto<UserLocationDto>> GetListAsync(Guid sessionUnitId);

    /// <summary>
    /// 停止共享位置（立即）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<bool> StopSharingAsync(StopShareLocationInput input);
}
