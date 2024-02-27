using IczpNet.Chat.BaseInfos;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.EntryNames.Dtos
{
    public class EntryNameDto : BaseTreeInfo<Guid>, IEntityDto<Guid>
    {
        //public virtual string Title { get; set; }

        public virtual string Code { get; set; }

        public virtual string InputType { get; set; }

        public virtual string Help { get; set; }

        public virtual string DefaultValue { get; set; }

        public virtual string Regex { get; set; }

        public virtual int? MaxLenth { get; set; }

        public virtual int? MinLenth { get; set; } = 1;

        public virtual int? MaxCount { get; set; }

        public virtual int? MinCount { get; set; } = 1;

        public virtual bool IsChoice { get; set; }

        public virtual bool IsUniqued { get; set; } = false;

        public virtual bool IsRequired { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsPublic { get; set; }

        [MaxLength(200)]
        public virtual string Description { get; set; }

        [MaxLength(200)]
        public virtual string ErrorMessage { get; set; }

    }
}
