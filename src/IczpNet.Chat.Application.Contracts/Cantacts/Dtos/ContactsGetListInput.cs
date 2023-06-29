using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Contacts.Dtos
{
    public class ContactsGetListInput : GetListInput
    {
        /// <summary>
        /// 所属聊天对象Id
        /// </summary>
        [Required]
        public virtual long? OwnerId { get; set; }

        /// <summary>
        /// 目标聊天对象类型
        /// </summary>
        public virtual ChatObjectTypeEnums? DestinationObjectType { get; set; }

        /// <summary>
        /// 联系人标签
        /// </summary>
        public virtual Guid? TagId { get; set; }

    }
}
