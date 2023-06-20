using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.SessionSections.SessionUnits.Dtos
{
    public class SessionUnitGetMessageListInput : GetListInput
    {
        /// <summary>
        /// 发送人【聊天对象】
        /// </summary>
        public virtual long? SenderId { get; set; }

        /// <summary>
        /// 是否有提醒
        /// </summary>
        public virtual bool? IsRemind { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public virtual MessageTypes? MessageType { get; set; }

        /// <summary>
        /// 是否转发的
        /// </summary>
        public virtual bool? IsFollowed { get; set; }

        /// <summary>
        /// 最小消息Id
        /// </summary>
        public virtual long? MinMessageId { get; set; }

        /// <summary>
        /// 最大消息Id
        /// </summary>
        public virtual long? MaxMessageId { get; set; }
    }
}
