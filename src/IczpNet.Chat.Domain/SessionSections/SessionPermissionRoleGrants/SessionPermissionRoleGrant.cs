using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionRoles;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionPermissionRoleGrants
{
    public class SessionPermissionRoleGrant : BaseEntity
    {
        protected SessionPermissionRoleGrant() { }

        public virtual string DefinitionId { get; set; }

        [ForeignKey(nameof(DefinitionId))]
        public virtual SessionPermissionDefinition Definition { get; set; }

        public virtual Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual SessionRole Role { get; set; }

        public virtual long Value { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { DefinitionId, RoleId, };
        }
    }
}
