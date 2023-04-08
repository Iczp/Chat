using System;
using IczpNet.Chat.SessionSections.Sessions.Dtos;
using IczpNet.Chat.SessionSections.SessionTags.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionSections.SessionTags;

public interface ISessionTagAppService :
    ICrudAppService<
        SessionTagDetailDto,
        SessionTagDto,
        Guid,
        SessionTagGetListInput,
        SessionTagCreateInput,
        SessionTagUpdateInput>
{
}
