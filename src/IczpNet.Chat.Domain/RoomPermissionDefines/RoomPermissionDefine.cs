using IczpNet.Chat.RoomPermissionGrants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Volo.Abp.Domain.Entities.Auditing;

namespace IczpNet.Chat.RoomPermissionDefines
{
    public class RoomPermissionDefine : AuditedAggregateRoot<string>
    {
        /// <summary>
        /// 父级Id
        /// </summary>
        [Key]
        [StringLength(100)]
        public override string Id { get; protected set; }
        /// <summary>
        /// 父级Id
        /// </summary>
        [StringLength(100)]
        public virtual string ParentId { get; set; }
        /// <summary>
        /// 分组名称
        /// </summary>
        [StringLength(50)]
        public virtual string GroupName { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        [StringLength(50)]
        [Required]
        //[Unique]
        public virtual string Name { get; set; }
        /// <summary>
        /// 数量类型
        /// </summary>
        [StringLength(50)]
        public virtual string DateType { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public virtual long DefaultValue { get; set; }
        /// <summary>
        /// 最大值
        /// </summary>
        public virtual long MaxValue { get; set; }
        /// <summary>
        /// 最小值
        /// </summary>
        public virtual long MinValue { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        [StringLength(200)]
        public virtual string Description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual long Sorting { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [ForeignKey(nameof(ParentId))]
        public virtual RoomPermissionDefine Parent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<RoomPermissionDefine> Childs { get; set; } = new List<RoomPermissionDefine>();
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<RoomPermissionGrant> GrantList { get; set; } = new List<RoomPermissionGrant>();

        protected RoomPermissionDefine()
        {

        }
        public RoomPermissionDefine(string id) : base(id)
        {
            Id = id.Trim();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public RoomPermissionDefine AddChilds(List<RoomPermissionDefine> items)
        {
            Childs = Childs.Concat(items).ToList();
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="permissionItem"></param>
        /// <returns></returns>
        public RoomPermissionDefine AddChild(RoomPermissionDefine permissionItem)
        {
            Childs.Add(permissionItem);
            return permissionItem;
        }
    }
}
