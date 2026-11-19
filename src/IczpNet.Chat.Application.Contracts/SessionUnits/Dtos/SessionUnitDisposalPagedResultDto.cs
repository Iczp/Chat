using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionUnits.Dtos;

/// <summary>
/// 会话处置增量返回分页结果
/// </summary>
public class SessionUnitDisposalPagedResultDto : PagedResultDto<SessionUnitDisposalDto>
{
    /// <summary>
    /// 服务端当前针对该 Owner 的最高水位 ID（用于客户端安全更新本地游标）
    /// </summary>
    public virtual long HeadId { get; set; }

    public SessionUnitDisposalPagedResultDto() { }

    public SessionUnitDisposalPagedResultDto(long totalCount, IReadOnlyList<SessionUnitDisposalDto> items, long headId)
        : base(totalCount, items)
    {
        HeadId = headId;
    }
}
