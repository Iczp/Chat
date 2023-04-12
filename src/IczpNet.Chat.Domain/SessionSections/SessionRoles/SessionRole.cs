using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionRoles
{
    public class SessionRole : BaseEntity<Guid>
    {
        protected SessionRole() { }

        public SessionRole(Guid id, Guid sessionId, string name) : base(id)
        {
            Name = name;
            SessionId = sessionId;
        }

        public virtual Guid? SessionId { get; protected set; }

        [ForeignKey(nameof(SessionId))]
        public virtual Session Session { get; set; }

        [StringLength(20)]
        public virtual string Name { get; set; }

        public virtual List<SessionUnitRole> SessionUnitRoleList { get; protected set; }

        public virtual IList<SessionPermissionRoleGrant> RoleGrantList { get; set; }
    }
}
