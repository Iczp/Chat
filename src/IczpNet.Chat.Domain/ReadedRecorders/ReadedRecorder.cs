using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.MessageSections.Messages;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.ReadedRecorders
{
    public class ReadedRecorder : BaseSessionEntity<Guid>, IDeviceId
    {
        ///// <summary>
        ///// AutoId 
        ///// </summary>
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        ////[Index]
        //public virtual long AutoId { get; set; }

        [StringLength(36)]
        public virtual string DeviceId { get; set; }

        public virtual long? MessageAutoId { get; protected set; }

        public virtual long? MessageId { get; protected set; }

        [ForeignKey(nameof(MessageId))]
        public virtual Message Message { get; protected set; }

        /// <summary>
        /// 会话Id
        /// </summary>
        [StringLength(100)]
        [Required]
        public virtual string SessionId { get; protected set; }

        /// <summary>
        /// 构造器
        /// </summary>
        protected ReadedRecorder() { }

        /// <summary>
        /// 设置会话ID
        /// </summary>
        internal virtual void SetSessionId(string sessionId)
        {
            SessionId = sessionId;
        }
    }
}
