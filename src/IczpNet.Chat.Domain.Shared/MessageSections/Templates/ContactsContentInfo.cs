using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 联系人消息
    /// </summary>
    public class ContactsContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        public virtual Guid DestinationId { get; set; }

        /// <summary>
        /// 联系人名称
        /// </summary>
        //[Index]

        public string Name { get; set; }

        public virtual string Code { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public virtual string Portrait { get; set; }

        public virtual ChatObjectTypes? ObjectType { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}