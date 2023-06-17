using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RedEnvelopes.Dtos
{
    /// <summary>
    /// GetUserRedEnvelopeDetailListOutput
    /// </summary>
    public class GetUserRedEnvelopeDetailListOutput : PagedResultDto<RedEnvelopeDetailForUserDto>
    {
        ///// <summary>
        ///// 时间（防止分页时，数据重复或遗落）
        ///// </summary>
        //public long NowTicks { get; set; }
    }
}
