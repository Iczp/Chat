using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.RedEnvelopes.Dtos
{
    /// <summary>
    /// GetUserRedEnvelopeDetailListInput
    /// </summary>
    public class GetUserRedEnvelopeDetailListInput : BaseGetListInput
    {
        /// <summary>
        /// 红包归属UserId
        /// </summary>
        public string OwnerUserId { get; set; }
        ///// <summary>
        ///// 时间（防止分页时，数据重复或遗落）
        ///// </summary>
        //public long? NowTicks { get; set; }
    }
}
