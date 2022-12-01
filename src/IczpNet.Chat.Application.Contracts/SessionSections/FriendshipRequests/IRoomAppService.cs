using IczpNet.Chat.SessionSections.FriendshipRequests.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.FriendshipRequests;

public interface IFriendshipRequestAppService :
    ICrudAppService<
        FriendshipRequestDetailDto,
        FriendshipRequestDto,
        Guid,
        FriendshipRequestGetListInput,
        FriendshipRequestCreateInput,
        FriendshipRequestUpdateInput>
{
}
