using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos
{
    /// <summary>
    /// 发送人会话单元
    /// </summary>
    public class SessionUnitSenderDto : SessionUnitSenderInfo
    {
        /// <summary>
        /// 是否好友
        /// </summary>
        public virtual bool? IsFriendship { get; set; }

        /// <summary>
        /// 好友名称
        /// </summary>
        public virtual string FriendshipName { get; set; }

        /// <summary>
        /// 好友会话Id
        /// </summary>
        public virtual Guid? FriendshipSessionUnitId { get; set; }

        public virtual SessionUnitSettingDto Setting { get; set; }

    }
}
