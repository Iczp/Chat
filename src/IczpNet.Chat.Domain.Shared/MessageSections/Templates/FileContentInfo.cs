namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 文件消息
    /// </summary>
    //[AutoMap(typeof(FileContent))]
    public class FileContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// FileName
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 文件地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 文件地址
        /// </summary>
        public string ActionUrl { get; set; }

        /// <summary>
        /// ContentType
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 文件后缀名
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// 大小 ContentLength(Size)
        /// </summary>
        public long? ContentLength { get; set; }
    }
}