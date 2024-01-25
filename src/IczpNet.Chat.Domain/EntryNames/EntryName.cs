using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.EntryValues;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.EntryNames
{
    [Index(nameof(Name))]
    [Index(nameof(Code))]
    [Index(nameof(InputType))]
    [Index(nameof(IsStatic))]
    [Index(nameof(FullPath))]
    public class EntryName : BaseTreeEntity<EntryName, Guid>, IIsStatic, IIsPublic
    {
        //[MaxLength(20)]
        //public virtual string Name { get; set; }

        [MaxLength(20)]
        public virtual string Code { get; set; }

        /// <summary>
        /// Text | Number | Textarea | Radio | Checkbox | Select | Switch | Slider | Upload | Date | Time |
        /// </summary>
        [MaxLength(20)]
        public virtual string InputType { get; set; }

        [MaxLength(500)]
        public virtual string Help { get; set; }

        [MaxLength(500)]
        public virtual string DefaultValue { get; set; } = null;

        [MaxLength(100)]
        public virtual string Regex { get; set; } = null;

        public virtual int? MaxLenth { get; set; } 

        public virtual int? MinLenth { get; set; }

        public virtual int? MaxCount { get; set; } 

        public virtual int? MinCount { get; set; } 

        public virtual bool IsChoice { get; set; }

        public virtual bool IsUniqued { get; set; } = false;

        public virtual bool IsRequired { get; set; } = false;

        public virtual bool IsStatic { get; set; } = false;

        public virtual bool IsPublic { get; set; } = true;

        //[MaxLength(200)]
        //public virtual string Description { get; set; }

        [MaxLength(200)]
        public virtual string ErrorMessage { get; set; }

        public virtual IList<EntryValue> EntryValueList { get; set; }


        protected EntryName() { }

        public EntryName(Guid id, string name, Guid? parentId) : base(id, name, parentId) { }
    }
}
