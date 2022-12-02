using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ChatObjects.Dtos
{
    public class ChatObjectSimpleDto : EntityDto<Guid>
    {
        public virtual string Name { get; set; }

        public virtual ChatObjectTypeEnum ObjectType { get; set; }
    }
}
