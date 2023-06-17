using IczpNet.Chat.ChatObjects.Dtos;
using System;

namespace IczpNet.Chat.RedEnvelopes.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    public class RedEnvelopeDetailForUserDto : RedEnvelopeDetailResult
    {
        /// <summary>
        /// 领红包的人
        /// </summary>
        public ChatObjectSimpleDto OwnerUser { get; set; }

        /// <summary>
        /// 发红包的人
        /// </summary>
        public ChatObjectSimpleDto ProviderUser { get; set; }
        /// <summary>
        /// 发红包的人
        /// </summary>
        public Guid ProviderUserId { get; set; }
    }
}
