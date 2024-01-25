using IczpNet.Chat.Entries.Dtos;
using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ChatObjects.Dtos
{
    public class ChatObjectDetailDto : ChatObjectDto
    {
        /// <summary>
        /// 设置加群、加好友、加聊天广场验证方式
        /// </summary>
        public virtual VerificationMethods VerificationMethod { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public virtual string Description { get; set; }

        ///// <summary>
        ///// 朋友数量
        ///// </summary>
        //public virtual int FriendCount { get; set; }

        ///// <summary>
        ///// 被动朋友数量
        ///// </summary>
        //public virtual int InFriendCount { get; set; }

        ///// <summary>
        ///// 发送者消息数量
        ///// </summary>
        //public virtual int SenderMessageCount { get; set;}

        ///// <summary>
        ///// 接收者消息数量（不包含群聊、公众号等）
        ///// </summary>
        //public virtual int ReceiverMessageCount { get; set; }

        public virtual List<EntryObjectDto> Entries { get; set; }

    }
}
