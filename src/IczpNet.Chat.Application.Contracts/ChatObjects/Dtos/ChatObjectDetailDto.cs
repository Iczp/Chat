using System;
using System.Collections.Generic;

namespace IczpNet.Chat.ChatObjects.Dtos
{
    public class ChatObjectDetailDto : ChatObjectDto
    {
        public virtual Guid? OwnerUserId { get; set; }

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

        /// <summary>
        /// 代理掌柜数量
        /// </summary>
        public virtual int ProxyShopKeeperCount { get; set; }

        /// <summary>
        /// 代理掌店小二
        /// </summary>
        public virtual int ProxyShopWaiterCount { get; set; }

        /// <summary>
        /// 加入群数量
        /// </summary>
        public virtual int InRoomMemberCount { get; set; }

        /// <summary>
        /// 被禁言次数
        /// </summary>
        public virtual int InRoomForbiddenMemberCount { get; set; }


        /// <summary>
        /// 接收服务号消息数量
        /// </summary>
        public virtual int InOfficialGroupMemberCount { get; set; }


        /// <summary>
        /// 订阅号数量
        /// </summary>
        public virtual int InOfficialMemberCount { get; set; }

        /// <summary>
        /// 公众号被拉黑次数
        /// </summary>
        public virtual int InOfficalExcludedMemberCount { get; set; }

        /// <summary>
        /// 订阅广场数量
        /// </summary>
        public virtual int InSquareMemberCount { get; set; }

    }
}
