using IczpNet.Chat.OfficialSections.OfficialMembers.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.OfficialSections.OfficialMembers;

public interface IOfficialMemberAppService :
    ICrudAppService<
        OfficialMemberDetailDto,
        OfficialMemberDto,
        Guid,
        OfficialMemberGetListInput,
        OfficialMemberCreateInput,
        OfficialMemberUpdateInput>
{
}
