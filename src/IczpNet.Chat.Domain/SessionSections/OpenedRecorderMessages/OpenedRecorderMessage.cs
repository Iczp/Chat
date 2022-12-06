using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.OpenedRecorders;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.SessionSections.OpenedRecorderMessages
{
    public class OpenedRecorderMessage //: BaseEntity
    {

        public virtual Guid MessageId { get; set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; set; }

        public virtual Guid OpenedRecorderId { get; set; }

        [ForeignKey(nameof(OpenedRecorderId))]
        public virtual OpenedRecorder OpenedRecorder { get; set; }

        /// <summary>
        /// Message.AutoId (Clone)
        /// </summary>
        public virtual long MessageAutoId { get; set; }

        protected OpenedRecorderMessage() { }

        //public override object[] GetKeys()
        //{
        //    return new object[] { MessageId, OpenedRecorderId };
        //}
    }
}
