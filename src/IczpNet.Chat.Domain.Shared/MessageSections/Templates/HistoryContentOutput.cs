namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 聊天历史
    /// </summary>
    //[AutoMapFrom(typeof(HistoryContent))]
    public class HistoryContentOutput : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 标题内容
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// 简要说明
        /// </summary>
        public virtual string Description { get; set; }
    }
}