using IczpNet.AbpCommons;
using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using IczpNet.Chat.Permissions;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionRoles;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IczpNet.Chat.SessionSections.SessionPermissionRoleGrants
{
    public class SessionPermissionRoleGrant : BaseEntity, IIsEnabled
    {
        protected SessionPermissionRoleGrant() { }

        public SessionPermissionRoleGrant(string definitionId,Guid roleId, long value, bool isEnabled)
        {
            Assert.If(!SessionPermissionDefinitionConsts.GetAll().Contains(definitionId), $"Key does not exist:{definitionId}");
            DefinitionId = definitionId;
            RoleId = roleId;
            Value = value;
            IsEnabled = isEnabled;
        }

        public virtual string DefinitionId { get; set; }

        [ForeignKey(nameof(DefinitionId))]
        public virtual SessionPermissionDefinition Definition { get; set; }

        public virtual Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual SessionRole Role { get; set; }

        public virtual long Value { get; set; }

        public virtual bool IsEnabled { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { DefinitionId, RoleId, };
        }
    }
}
