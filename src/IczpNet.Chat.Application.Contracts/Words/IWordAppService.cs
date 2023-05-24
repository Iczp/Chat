using IczpNet.Chat.Words.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.Words
{
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
}
