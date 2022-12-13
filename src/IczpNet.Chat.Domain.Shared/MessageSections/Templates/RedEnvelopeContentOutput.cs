namespace IczpNet.Chat.MessageSections.Templates
{
    public class RedEnvelopeContentOutput : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public virtual string Text { get; set; }

        ///// <summary>
        ///// 到期时间
        ///// </summary>
        //public virtual DateTime ExpireTime { get; set; }

        ///// <summary>
        ///// 登录人的领取详细
        ///// </summary>
        //public virtual RedEnvelopeDetailResult Detail { get; set; }

    }
}
