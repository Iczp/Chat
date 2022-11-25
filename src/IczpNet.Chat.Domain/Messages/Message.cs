using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.Messages
{
    public partial class Message : BaseEntity<Guid>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual long AutoId { get; set; }

        //[Required]
        public virtual Guid? SenderId { get; set; }

        //[Required]
        public virtual Guid? ReceiverId { get; set; }

        public virtual MessageChannel MessageChannel { get; set; }

        public virtual MessageType MessageType { get; set; }

        /// <summary>
        /// 扩展（键名）根据业务自义，如:"courseId"、"course-userId"、"erp-userId"
        /// </summary>
        [StringLength(100)]
        public virtual string KeyName { get; set; }

        /// <summary>
        /// 扩展（键值）根据业务自义,如："123456789"、"02b7d668-02ca-428f-b88c-b8adac2c5044"、"admin"
        /// </summary>
        [StringLength(5000)]
        public virtual string KeyValue { get; set; }

        /// <summary>
        /// 撤回消息时间
        /// </summary>
        public virtual DateTime? RollbackTime { get; set; }

        [ForeignKey(nameof(SenderId))]
        public virtual ChatObject Sender { get; set; }

        [ForeignKey(nameof(ReceiverId))]
        public virtual ChatObject Receiver { get; set; }

    }
}
