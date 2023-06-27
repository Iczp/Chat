﻿using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionSections.SessionTags;
using IczpNet.Chat.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionUnitTags
{
    public class SessionUnitTag : BaseEntity
    {
        public virtual Guid SessionUnitId { get; set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; set; }

        public virtual Guid SessionTagId { get; set; }

        [ForeignKey(nameof(SessionTagId))]
        public virtual SessionTag SessionTag { get; set; }

        protected SessionUnitTag() { }

        public SessionUnitTag(SessionTag sessionTag, SessionUnit sessionUnit)
        {
            SessionTag = sessionTag;
            SessionUnit = sessionUnit;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, SessionTagId };
        }
    }
}
