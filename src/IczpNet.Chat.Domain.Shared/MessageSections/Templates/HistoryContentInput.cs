using System;
using System.Collections.Generic;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 聊天历史
    /// </summary>
    public class HistoryContentInput : BaseMessageContentInfo, IMessageContentInfo
    {
        ///// <summary>
        ///// 标题内容
        ///// </summary>
        //public virtual string Title { get; set; }
        ///// <summary>
        ///// 简要说明
        ///// </summary>
        //public virtual string Description { get; set; }
        /// <summary>
        /// 消息Id列表
        /// </summary>
        public virtual IList<Guid> MessageIdList { get; set; }
    }
}