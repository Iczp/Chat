using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.EntryValues;
using IczpNet.Chat.SessionUnits;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionUnitEntryValues;

[Index(nameof(CreationTime))]
public class SessionUnitEntryValue : BaseEntity
{
    public virtual Guid SessionUnitId { get; set; }

    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; set; }

    public virtual Guid EntryValueId { get; set; }

    [ForeignKey(nameof(EntryValueId))]
    public virtual EntryValue EntryValue { get; set; }

    //[MaxLength(100)]
    //public virtual string Value { get; set; }

    public override object[] GetKeys()
    {
        return [SessionUnitId, EntryValueId];
    }
}
