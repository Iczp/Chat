using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.EntryValues.Dtos
{
    public class EntryValueDto : EntityDto<Guid>
    {
        public virtual string Value { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsPublic { get; set; }

    }
}
