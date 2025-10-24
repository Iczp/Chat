using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Friends;

/// <summary>
/// 好友在线状态
/// </summary>
public interface IFriendsAppService
{

    Task<PagedResultDto<FriendStatusDto>> GetListOnlineAsync(FriendStatusGetListInput input);

    Task<PagedResultDto<FriendStatusDto>> GetListOnlineByCurrentUserAsync(FriendStatusGetListInput input);
}
