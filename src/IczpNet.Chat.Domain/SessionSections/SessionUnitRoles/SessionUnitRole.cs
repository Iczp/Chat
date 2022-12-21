using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionUnitRoles
{
    public class SessionUnitRole : BaseEntity
    {
        public virtual Guid SessionUnitId { get; set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; set; }

        public virtual Guid SessionRoleId { get; set; }

        [ForeignKey(nameof(SessionRoleId))]
        public virtual SessionRole SessionRole { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, SessionRoleId };
        }
    }
}
