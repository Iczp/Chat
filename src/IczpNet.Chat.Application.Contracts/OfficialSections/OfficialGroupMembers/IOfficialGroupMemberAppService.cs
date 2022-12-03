using IczpNet.Chat.OfficialSections.OfficialGroupMembers.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.OfficialSections.OfficialGroupMembers;

public interface IOfficialGroupMemberAppService :
    ICrudAppService<
        OfficialGroupMemberDetailDto,
        OfficialGroupMemberDto,
        Guid,
        OfficialGroupMemberGetListInput,
        OfficialGroupMemberCreateInput,
        OfficialGroupMemberUpdateInput>
{
}
