namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// 分享链接消息
    /// </summary>
    //[AutoMap(typeof(LinkContent))]
    public class LinkContentInfo : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 简要说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 发行人名称
        /// </summary>
        public string IssuerName { get; set; }

        /// <summary>
        /// 发行人图标
        /// </summary>
        public string IssuerIcon { get; set; }
    }
}