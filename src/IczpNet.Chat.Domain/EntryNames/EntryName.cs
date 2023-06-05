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
    public class EntryName : BaseEntity<Guid>, IIsStatic, IIsPublic
    {
        [MaxLength(20)]
        public virtual string Name { get; set; }

        [MaxLength(20)]
        public virtual string Code { get; set; }

        public virtual bool IsStatic { get; set; }

        public virtual bool IsPublic { get; set; }

        public virtual IList<EntryValue> EntryValueList { get; set; }

        protected EntryName() { }

        public EntryName(Guid id) : base(id) { }
    }
}
