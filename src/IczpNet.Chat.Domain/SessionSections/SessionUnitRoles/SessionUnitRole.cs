using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.SessionSections.SessionRoles;
using IczpNet.Chat.SessionUnits;
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

        protected SessionUnitRole() { }

        public SessionUnitRole(SessionRole sessionRole, SessionUnit sessionUnit)
        {
            SessionRole = sessionRole;
            SessionUnit = sessionUnit;
        }

        public override object[] GetKeys()
        {
            return new object[] { SessionUnitId, SessionRoleId };
        }
    }
}
