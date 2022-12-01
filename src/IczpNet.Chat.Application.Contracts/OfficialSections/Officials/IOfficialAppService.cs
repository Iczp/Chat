using IczpNet.Chat.OfficialSections.Officials.Dtos;
using System;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.OfficialSections.Officials;

public interface IOfficialAppService :
    ICrudAppService<
        OfficialDetailDto,
        OfficialDto,
        Guid,
        OfficialGetListInput,
        OfficialCreateInput,
        OfficialUpdateInput>
{
}
