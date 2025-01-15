using IczpNet.Chat.Words.Dtos;
using System;

namespace IczpNet.Chat.Words;

public interface IWordAppService
    : ICrudChatAppService<
        WordDetailDto,
        WordDto,
        Guid,
        WordGetListInput,
        WordCreateInput,
        WordUpdateInput>
{


}
