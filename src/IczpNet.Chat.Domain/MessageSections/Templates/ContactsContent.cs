using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates
{
    public class ContactsContent : BaseMessageContentEntity, IChatOwner<Guid?>
    {
        public Guid? OwnerId { get; set; }

        public ChatObject Owner { get; set; }

        /// <summary>
        /// 联系人名称
        /// </summary>
        //[Index]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public virtual string Code { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [StringLength(300)]
        [MaxLength(300)]
        public virtual string Portrait { get; set; }

        public virtual ChatObjectTypeEnum? ObjectType { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(200)]
        public string Remark { get; set; }
    }
}
