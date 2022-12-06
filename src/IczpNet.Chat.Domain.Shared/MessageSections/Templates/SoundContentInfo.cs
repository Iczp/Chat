namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 语音消息
    /// </summary>
    //[AutoMap(typeof(SoundContent))]
    public class SoundContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 语音地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 手机的存储路径(前端使用字段，服务端不能存这个字段)
        /// </summary>

        public string Path { get; set; }

        /// <summary>
        /// 语音的文本内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 语音时长（毫秒）
        /// </summary>
        public int Time { get; set; }
    }
}