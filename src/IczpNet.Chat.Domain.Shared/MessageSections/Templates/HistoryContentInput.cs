using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 聊天历史
    /// </summary>
    public class HistoryContentInput : BaseMessageContentInfo, IContentInfo
    {
        /// <summary>
        /// 消息Id列表
        /// </summary>
        public virtual IList<long> MessageIdList { get; set; }
    }
}