using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.ContactTags;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionUnitContactTags;

public class SessionUnitContactTag : BaseEntity
{
    public virtual Guid SessionUnitId { get; set; }

    [ForeignKey(nameof(SessionUnitId))]
    public virtual SessionUnit SessionUnit { get; set; }

    public virtual Guid TagId { get; set; }

    [ForeignKey(nameof(TagId))]
    public virtual ContactTag Tag { get; set; }

    protected SessionUnitContactTag() { }

    public SessionUnitContactTag(SessionUnit sessionUnit, ContactTag sessionTag)
    {
        Tag = sessionTag;
        SessionUnit = sessionUnit;
    }

    public override object[] GetKeys()
    {
        return new object[] { SessionUnitId, TagId };
    }
}
