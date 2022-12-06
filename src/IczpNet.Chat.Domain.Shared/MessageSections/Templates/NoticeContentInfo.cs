using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 文本消息
    /// </summary>
    public class NoticeContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public virtual NoticeTypes NoticeType { get; set; }

        /// <summary>
        /// 图标
        /// </summary>

        public virtual string Icon { get; set; }
        /// <summary>
        /// 通知标题
        /// </summary>

        //[Index]
        public virtual string Title { get; set; }

        /// <summary>
        /// 通知标题
        /// </summary>

        public virtual string Url { get; set; }
        /// <summary>
        /// 通知内容
        /// </summary>

        public virtual string Content { get; set; }

        /// <summary>
        /// 提醒时间
        /// </summary>
        public virtual DateTime? RemindTime { get; set; }

    }
}