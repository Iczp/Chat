using IczpNet.Chat.Follows.Dtos;
using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Follows
{
    /// <summary>
    /// 关注
    /// </summary>
    public interface IFollowAppService
    {
        /// <summary>
        /// 我关注的
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<SessionUnitDestinationDto>> GetListFollowingAsync(FollowingGetListInput input);

        /// <summary>
        /// 关注我的
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<PagedResultDto<SessionUnitDestinationDto>> GetListFollowerAsync(FollowerGetListInput input);

        /// <summary>
        /// 添加关注
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<bool> CreateAsync(FollowCreateInput input);

        /// <summary>
        /// 取消关注
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        Task DeleteAsync(FollowDeleteInput input);
    }
}
