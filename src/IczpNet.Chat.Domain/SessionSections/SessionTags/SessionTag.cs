using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionTags
{
    public class SessionTag : BaseEntity<Guid>
    {
        public SessionTag(Guid id, string name) : base(id)
        {
            Name = name;
        }

        public virtual Guid? SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        [StringLength(20)]
        public virtual string Name { get; set; }

        public virtual IList<SessionUnitTag> SessionUnitTagList { get; protected set; }
    }
}
