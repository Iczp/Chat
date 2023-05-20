using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 联系人消息
    /// </summary>
    public class ContactsContentInfo : BaseMessageContentInfo, IContentInfo
    {
        public virtual long DestinationId { get; set; }

        /// <summary>
        /// 联系人名称
        /// </summary>
        //[Index]

        public virtual string Name { get; set; }

        public virtual string Code { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public virtual string Portrait { get; set; }

        public virtual ChatObjectTypeEnums? ObjectType { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Remark { get; set; }
    }
}