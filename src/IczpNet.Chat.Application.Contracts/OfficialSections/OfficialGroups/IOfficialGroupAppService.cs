using IczpNet.Chat.OfficialSections.OfficialGroups.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.OfficialSections.OfficialGroups;

public interface IOfficialGroupAppService :
    ICrudAppService<
        OfficialGroupDetailDto,
        OfficialGroupDto,
        Guid,
        OfficialGroupGetListInput,
        OfficialGroupCreateInput,
        OfficialGroupUpdateInput>
{
}
