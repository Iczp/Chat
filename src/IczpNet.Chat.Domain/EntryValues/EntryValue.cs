using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjectEntryValues;
using IczpNet.Chat.EntryNames;
using IczpNet.Chat.SessionSections.SessionUnitEntryValues;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.EntryValues
{
    [Index(nameof(Value))]
    [Index(nameof(CreationTime))]
    [Index(nameof(EntryNameId), nameof(Value))]
    public class EntryValue : BaseEntity<Guid>
    {
        public virtual Guid EntryNameId { get; set; }

        [ForeignKey(nameof(EntryNameId))]
        public virtual EntryName EntryName { get; set; }

        [MaxLength(500)]
        public virtual string Value { get; set; }

        public virtual IList<SessionUnitEntryValue> SessionUnitEntryValueList { get; set; }

        public virtual IList<ChatObjectEntryValue> ChatObjectEntryValueList { get; set; }

        protected EntryValue() { }

        public EntryValue(Guid id, Guid entryNameId, string value) : base(id)
        {
            EntryNameId = entryNameId;
            Value = value;
        }
    }
}
