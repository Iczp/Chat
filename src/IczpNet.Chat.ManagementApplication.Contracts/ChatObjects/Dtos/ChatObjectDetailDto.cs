using System;

namespace IczpNet.Chat.Management.ChatObjects.Dtos
{
    public class ChatObjectDetailDto : ChatObjectDto
    {
        public virtual string Description { get; set; }

        /// <summary>
        /// 朋友数量
        /// </summary>
        public virtual int FriendCount { get; set; }

        /// <summary>
        /// 被动朋友数量
        /// </summary>
        public virtual int InFriendCount { get; set; }

        /// <summary>
        /// 发送者消息数量
        /// </summary>
        public virtual int SenderMessageCount { get; set;}

        /// <summary>
        /// 接收者消息数量（不包含群聊、公众号等）
        /// </summary>
        public virtual int ReceiverMessageCount { get; set; }

    }
}
