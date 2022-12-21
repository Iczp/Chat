using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionSections.SessionUnitTags;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionTags
{
    public class SessionTag : BaseEntity<Guid>
    {

        public virtual Guid? SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        [StringLength(20)]
        public virtual string Name { get; set; }

        public virtual List<SessionUnitTag> SessionUnitTagList { get; protected set; }
    }
}
