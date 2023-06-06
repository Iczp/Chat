using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.EntryValues.Dtos
{
    public class EntryValueSimpleDto : EntityDto<Guid>
    {
        public virtual string Value { get; set; }
    }
}
