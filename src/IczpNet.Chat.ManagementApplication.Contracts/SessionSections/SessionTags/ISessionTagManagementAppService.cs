using System;
using IczpNet.Chat.Management.SessionSections.SessionTags.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.Management.SessionSections.SessionTags;

public interface ISessionTagManagementAppService :
    ICrudAppService<
        SessionTagDetailDto,
        SessionTagDto,
        Guid,
        SessionTagGetListInput,
        SessionTagCreateInput,
        SessionTagUpdateInput>
{
}
