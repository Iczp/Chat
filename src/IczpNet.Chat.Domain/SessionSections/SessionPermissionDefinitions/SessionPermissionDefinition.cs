using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SessionSections.SessionPermissionGroups;
using IczpNet.Chat.SessionSections.SessionPermissionRoleGrants;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions
{
    public class SessionPermissionDefinition : BaseEntity<string>, IIsEnabled
    {
        protected SessionPermissionDefinition() { }

        public SessionPermissionDefinition(string id) : base(id)
        {
        }

        [Key]
        [StringLength(100)]
        public override string Id { get; protected set; }

        public virtual long? GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual SessionPermissionGroup Group { get; set; }

        [StringLength(50)]
        [Required]
        //[Unique]
        public virtual string Name { get; set; }

        [StringLength(200)]
        public virtual string Description { get; set; }

        //[StringLength(50)]
        //public virtual string DateType { get; set; }

        public virtual long DefaultValue { get; set; }

        public virtual long MaxValue { get; set; }

        public virtual long MinValue { get; set; }

        public virtual long Sorting { get; set; }

        public virtual bool IsEnabled { get; set; }

        public virtual IList<SessionPermissionRoleGrant> RoleGrantList { get; set; }

        public virtual IList<SessionPermissionUnitGrant> UnitGrantList { get; set; }
    }
}
