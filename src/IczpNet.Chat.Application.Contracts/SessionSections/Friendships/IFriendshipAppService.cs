using IczpNet.Chat.SessionSections.Friendships.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.Friendships;

public interface IFriendshipAppService :
    ICrudAppService<
        FriendshipDetailDto,
        FriendshipDto,
        Guid,
        FriendshipGetListInput,
        FriendshipCreateInput,
        FriendshipUpdateInput>
{

    Task<string> SetRenameAsync(Guid friendshipId, string rename);
    Task<string> SetRemarksAsync(Guid friendshipId, string remarks);
    Task<bool> SetIsCantactsAsync(Guid friendshipId, bool isCantacts);
    Task<bool> SetIsImmersedAsync(Guid friendshipId, bool isImmersed);
    Task<bool> SetIsShowMemberNameAsync(Guid friendshipId, bool isShowMemberName);
    Task<bool> SetIsShowReadNameAsync(Guid friendshipId, bool isShowRead);
    Task<string> SetBackgroundImageAsync(Guid friendshipId, string backgroundImage);
}
