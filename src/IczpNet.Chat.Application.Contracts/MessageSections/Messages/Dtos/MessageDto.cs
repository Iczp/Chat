using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos
{
    public class MessageDto : MessageInfo<dynamic>, IEntityDto<long>
    {
        public virtual int SessionUnitCount { get; set; }

        public virtual int ReadedCount { get; set; }

        public virtual int OpenedCount { get; set; }

        public virtual int FavoritedCount { get; set; }
    }
}
