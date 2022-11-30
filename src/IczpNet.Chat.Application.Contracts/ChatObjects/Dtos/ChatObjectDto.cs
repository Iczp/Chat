using IczpNet.Chat.BaseDtos;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ChatObjects.Dtos
{
    public class ChatObjectDto : BaseDto<Guid>
    {
        public virtual long AutoId { get; set; }

        public virtual string Name { get; set; }

        public virtual string Code { get; set; }

        public virtual string ChatObjectType { get; set; }
    }
}
