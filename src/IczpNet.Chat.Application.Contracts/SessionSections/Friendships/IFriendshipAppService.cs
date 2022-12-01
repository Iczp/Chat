using IczpNet.Chat.SessionSections.Friendships.Dtos;
using System;
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
}
