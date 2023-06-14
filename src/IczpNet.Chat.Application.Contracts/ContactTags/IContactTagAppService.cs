using IczpNet.Chat.ContactTags.Dtos;
using System;

namespace IczpNet.Chat.ContactTags
{
    public interface IContactTagAppService
        : ICrudChatAppService<
            ContactTagDetailDto,
            ContactTagDto,
            Guid,
            ContactTagGetListInput,
            ContactTagCreateInput,
            ContactTagUpdateInput>
    {


    }
}
