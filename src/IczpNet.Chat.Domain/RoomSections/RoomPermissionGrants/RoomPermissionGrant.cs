using IczpNet.Chat.Attributes;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.RoomSections.RoomPermissionDefines;
using IczpNet.Chat.RoomSections.RoomRoles;
using IczpNet.Chat.SessionSections.Sessions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.RoomSections.RoomPermissionGrants
{

    [HasKey(nameof(RoleId), nameof(DefineId))]
    public class RoomPermissionGrant : BaseEntity
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        public virtual Guid RoleId { get; private set; }
        /// <summary>
        /// 角色Id
        /// </summary>
        [StringLength(100)]
        public virtual string DefineId { get; private set; }
        /// <summary>
        /// 权限值
        /// </summary>
        public virtual long Value { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        [ForeignKey(nameof(RoleId))]
        public virtual RoomRole Role { get; set; }
        /// <summary>
        /// 权限定义
        /// </summary>
        [ForeignKey(nameof(DefineId))]
        public virtual RoomPermissionDefine Define { get; set; }

        protected RoomPermissionGrant()
        {

        }
        public RoomPermissionGrant(Guid roleId, string defineId, long value)
        {
            RoleId = roleId;
            DefineId = defineId;
            Value = value;
        }

        public override object[] GetKeys()
        {
            return new object[] { RoleId, DefineId };
        }
    }
}
