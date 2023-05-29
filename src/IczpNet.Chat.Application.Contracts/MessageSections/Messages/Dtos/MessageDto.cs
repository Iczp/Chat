using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos
{
    public class MessageDto : MessageInfo<dynamic>, IEntityDto<long>
    {
        //public virtual long SessionUnitCount { get; set; }

        //public virtual long ReadedCount { get; set; }

        //public virtual long OpenedCount { get; set; }

        //public virtual long FavoritedCount { get; set; }
    }
}
