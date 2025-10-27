using IczpNet.Chat.BaseAppServices;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Users;

namespace IczpNet.Chat.Friends;

public class FriendsAppService(IFriendsManager friendManager) : ChatAppService, IFriendsAppService
{
    public IFriendsManager FriendManager { get; } = friendManager;

    public async Task<PagedResultDto<FriendStatusDto>> GetListOnlineAsync(FriendStatusGetListInput input)
    {
        if (!input.UserId.HasValue)
        {
            throw new ArgumentNullException(nameof(input.UserId));
        }

        var list = await FriendManager.GetListOnlineAsync(input.UserId.Value);
        var queryable = list.AsQueryable();
        return await GetPagedListAsync<FriendStatus, FriendStatusDto>(queryable, input);
    }

    public Task<PagedResultDto<FriendStatusDto>> GetListOnlineByCurrentUserAsync(FriendStatusGetListInput input)
    {
        var currentUserId = CurrentUser.GetId();
        if (input.UserId != null && input.UserId!= currentUserId)
        {
           throw new UserFriendlyException("Cannot specify a different UserId for the current user.");
        }
        input.UserId = CurrentUser.GetId();

        return GetListOnlineAsync(input);
    }
}
