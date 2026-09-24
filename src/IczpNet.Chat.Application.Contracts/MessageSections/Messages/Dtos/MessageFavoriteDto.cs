using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageFavoriteDto : MessageInfo<object>, IEntityDto<long>
{
    

    public virtual string GetObjectKey() => $"{Id}";
}
