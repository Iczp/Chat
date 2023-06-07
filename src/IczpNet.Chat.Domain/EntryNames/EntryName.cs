using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.EntryValues;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.EntryNames
{
    [Index(nameof(Name))]
    [Index(nameof(Code))]
    [Index(nameof(IsStatic))]
    [Index(nameof(FullPath))]
    public class EntryName : BaseTreeEntity<EntryName, Guid>, IIsStatic, IIsPublic
    {
        //[MaxLength(20)]
        //public virtual string Name { get; set; }

        [MaxLength(20)]
        public virtual string Code { get; set; }

        [MaxLength(100)]
        public virtual string Regex { get; set; } = null;

        public virtual int MaxLenth { get; set; } = 1;

        public virtual int MinLenth { get; set; } = 1;

        public virtual int MaxCount { get; set; } = 1;

        public virtual int MinCount { get; set; } = 1;

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
