using System;

namespace IczpNet.Chat.MessageSections.Templates
{
    /// <summary>
    /// RedEnvelopeContentOutput
    /// </summary>
    public class RedEnvelopeContentOutput : BaseMessageContentInfo, IMessageContentInfo
    {
        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text { get; set; }

        ///// <summary>
        ///// 到期时间
        ///// </summary>
        //public DateTime ExpireTime { get; set; }

        ///// <summary>
        ///// 登录人的领取详细
        ///// </summary>
        //public RedEnvelopeDetailResult Detail { get; set; }

    }
}
