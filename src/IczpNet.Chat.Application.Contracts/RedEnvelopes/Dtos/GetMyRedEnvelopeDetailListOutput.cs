using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.RedEnvelopes.Dtos;

/// <summary>
/// GetMyRedEnvelopeDetailListOutput
/// </summary>
public class GetMyRedEnvelopeDetailListOutput : PagedResultDto<UserRedEnvelopeDto>
{
    ///// <summary>
    ///// 领红包的人
    ///// </summary>
    //public ChatObjectSimpleDto OwnerUser { get; set; }
    ///// <summary>
    ///// 时间（防止分页时，数据重复或遗落）
    ///// </summary>
    //public long NowTicks { get; set; }
    /// <summary>
    /// 总金额
    /// </summary>
    public decimal TotalAmount { get; set; }
    /// <summary>
    /// 年份
    /// </summary>
    public List<int> YearList { get; set; }
    /// <summary>
    /// 最佳次数
    /// </summary>
    public int TopCount { get; set; }
}
