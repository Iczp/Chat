namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 文本消息
    /// </summary>
    public class TextContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        //[Required(ErrorMessage = "文本内容[Text]必填！")]
        public string Text { get; set; }

    }
}