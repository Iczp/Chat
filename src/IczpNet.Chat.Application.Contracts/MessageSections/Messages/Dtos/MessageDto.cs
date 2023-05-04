using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos
{
    public class MessageDto : MessageInfo<dynamic>, IEntityDto<long>
    {
        
    }
}
