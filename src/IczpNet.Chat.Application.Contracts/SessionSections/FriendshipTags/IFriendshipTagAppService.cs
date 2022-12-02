using IczpNet.Chat.FriendshipTagSections.FriendshipTags.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.FriendshipTagSections.FriendshipTags;

public interface IFriendshipTagAppService :
    ICrudAppService<
        FriendshipTagDetailDto,
        FriendshipTagDto,
        Guid,
        FriendshipTagGetListInput,
        FriendshipTagCreateInput,
        FriendshipTagUpdateInput>
{
}
