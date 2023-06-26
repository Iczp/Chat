using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos
{
    public class SessionRequestHandleInput
    {
        /// <summary>
        /// 主键Id
        /// </summary>
        [Required]
        public virtual Guid SessionRequestId { get; set; }

        /// <summary>
        /// 是否同意加好友/加入聊天广场/加入群聊
        /// </summary>
        [Required] 
        public virtual bool IsAgreed { get; set; }

        /// <summary>
        /// 处理消息
        /// </summary>
        public virtual string HandleMessage { get; set; }
    }
}
