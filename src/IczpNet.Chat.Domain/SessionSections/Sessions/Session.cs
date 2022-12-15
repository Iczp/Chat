using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class Session : BaseSessionEntity
    {

        public virtual ChatObjectTypes DestinationObjectType { get; set; }

        [StringLength(50)]
        public virtual string Title { get; set; }

        [StringLength(100)]
        public virtual string Description { get; set; }

        public virtual int Badge { get; set; }

        /// <summary>
        /// Set top
        /// </summary>
        public virtual double Sorting { get; set; }

        /// <summary>
        /// MaxAutoId
        /// </summary>
        public virtual long MessageAutoId { get; set; }

        public virtual Guid? MessageId { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; }

        public virtual DateTime ShowTime { get; set; }

    }
}
