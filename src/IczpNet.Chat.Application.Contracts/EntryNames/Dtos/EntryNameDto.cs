using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.BaseInfos;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.EntryNames.Dtos
{
    public class EntryNameDto : BaseTreeInfo<Guid>, IEntityDto<Guid>
    {
        //public virtual string Name { get; set; }

        public virtual string Code { get; set; }

    }
}
