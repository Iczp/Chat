using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionPermissionRoleGrants
{
    public class SessionPermissionUnitGrant : BaseEntity
    {
        protected SessionPermissionUnitGrant() { }

        public virtual string DefinitionId { get; set; }

        [ForeignKey(nameof(DefinitionId))]
        public virtual SessionPermissionDefinition Definition { get; set; }

        public virtual Guid SessionUnitId { get; set; }

        [ForeignKey(nameof(SessionUnitId))]
        public virtual SessionUnit SessionUnit { get; set; }

        public virtual long Value { get; set; }

        public virtual bool IsEnabled { get; set; }

        public override object[] GetKeys()
        {
            return new object[] { DefinitionId, SessionUnitId, };
        }
    }
}
