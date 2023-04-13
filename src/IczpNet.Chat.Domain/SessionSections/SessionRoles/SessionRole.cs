using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionPermissions;
using IczpNet.Chat.SessionSections.Sessions;
using IczpNet.Chat.SessionSections.SessionUnitRoles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.SessionSections.SessionRoles
{
    [Index(nameof(IsDefault), AllDescending = true)]
    public class SessionRole : BaseEntity<Guid>, IName, IIsDefault
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

        [StringLength(500)]
        public virtual string Description { get; set; }

        public virtual bool IsDefault { get; set; }

        public virtual List<SessionUnitRole> SessionUnitRoleList { get; protected set; }

        public virtual IList<SessionPermissionRoleGrant> RoleGrantList { get; set; }

        public void SetPermissionGrant(Dictionary<string, PermissionGrantValue> permissionGrant)
        {
            RoleGrantList?.Clear();
            RoleGrantList = permissionGrant.Select(x => new SessionPermissionRoleGrant(x.Key, x.Value.Value,x.Value.IsEnabled)).ToList();
        }

        public Dictionary<string, SessionPermissionRoleGrant> PermissionGrant => GetPermissionGrant();

        protected Dictionary<string, SessionPermissionRoleGrant> GetPermissionGrant()
        {
            return RoleGrantList?.ToDictionary(x => x.DefinitionId);
        }
    }
}
