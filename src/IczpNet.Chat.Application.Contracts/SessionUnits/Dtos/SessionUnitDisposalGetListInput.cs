using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionUnits.Dtos;

/// <summary>
/// 增量拉取会话处置记录输入参数
/// </summary>
public class SessionUnitDisposalGetListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 所属聊天对象 ID
    /// </summary>
    [Required]
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// 客户端本地已记录的最大处置记录 ID。仅拉取大于该 ID 的记录。
    /// 若首次登录无缓存，则不调用本接口。
    /// </summary>
    public virtual long AfterId { get; set; } = 0;
}
