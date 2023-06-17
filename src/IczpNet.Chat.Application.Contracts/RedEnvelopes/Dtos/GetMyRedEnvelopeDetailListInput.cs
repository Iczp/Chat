using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.RedEnvelopes.Dtos
{
    /// <summary>
    /// GetMyRedEnvelopeDetailListInput
    /// </summary>
    public class GetMyRedEnvelopeDetailListInput : BaseGetListInput
    {
        /// <summary>
        /// 红包归属UserId
        /// </summary>
        public Guid OwnerUserId { get; set; }

        /// <summary>
        /// 年
        /// </summary>
        public int? Year { get; set; }
    }
}
