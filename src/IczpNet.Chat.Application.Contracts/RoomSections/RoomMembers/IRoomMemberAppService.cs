using IczpNet.Chat.RoomSections.RoomMembers.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.RoomSections.RoomMembers;

public interface IRoomMemberAppService :
    ICrudAppService<
        RoomMemberDetailDto,
        RoomMemberDto,
        Guid,
        RoomMemberGetListInput,
        RoomMemberCreateInput,
        RoomMemberUpdateInput>
{
}
