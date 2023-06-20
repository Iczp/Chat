using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionSections.SessionTags.Dtos
{
    public class SessionTagGetListInput : BaseGetListInput
    {
        /// <summary>
        /// 会话单元Id
        /// </summary>
        public virtual Guid SessionId { get; set; }
    }
}
