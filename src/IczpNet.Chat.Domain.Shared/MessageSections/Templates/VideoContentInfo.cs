namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 视频消息
    /// </summary>
    public class VideoContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 视频地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 视频封面Width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 视频Height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 视频大小
        /// </summary>
        public double Size { get; set; }

        /// <summary>
        /// 视频封面
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// 视频Width
        /// </summary>
        public int ImageWidth { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// 封面大小
        /// </summary>
        public double ImageSize { get; set; }

        /// <summary>
        /// 选定视频的时间长度，单位为 （毫秒）
        /// </summary>
        public double Duration { get; set; }

    }
}