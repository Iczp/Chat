using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.EntryValues.Dtos
{
    public class EntryValueDto : EntityDto<Guid>
    {
        public virtual string Name { get; set; }

        public virtual Guid? EntryNameId { get; set; }

        public virtual string Value { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual bool IsOption { get; set; }

    }
}
