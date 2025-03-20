using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionTags;

public class SessionTag : BaseEntity<Guid>, ISessionId
{
    protected SessionTag() {}

    public SessionTag(Guid id, Guid sessionId, string name) : base(id)
    {
        Name = name;
        SessionId = sessionId;
    }

    public virtual Guid? SessionId { get; set; }

    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; }

    [StringLength(20)]
    public virtual string Name { get; set; }

    public virtual IList<SessionUnitTag> SessionUnitTagList { get; protected set; }
}
