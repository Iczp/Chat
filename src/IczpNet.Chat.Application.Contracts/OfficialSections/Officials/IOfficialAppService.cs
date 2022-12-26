using IczpNet.Chat.OfficialSections.Officials.Dtos;
using System;
using System.Threading.Tasks;
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

    Task<int> AddAllAsync(Guid id);
}
