using IczpNet.Chat.ChatObjects.Dtos;

namespace IczpNet.Chat.RedEnvelopes.Dtos
{
    /// <summary>
    /// 
    /// </summary>
    //[AutoMapFrom(typeof(RedEnvelopeDetail))]
    public class RedEnvelopeDetailAndUserDto : RedEnvelopeDetailResult
    {
        /// <summary>
        /// 归属人
        /// </summary>
        public ChatObjectSimpleDto OwnerUser { get; set; }
    }
}
