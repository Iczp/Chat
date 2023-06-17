using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.RedEnvelopes.Dtos
{
    /// <summary>
    /// GetRedEnvelopeDetailListInput
    /// </summary>
    public class GetRedEnvelopeDetailListInput : BaseGetListInput
    {
        /// <summary>
        /// 红包d
        /// </summary>
        public Guid RedEnvelopeContentId { get; set; }
    }
}
