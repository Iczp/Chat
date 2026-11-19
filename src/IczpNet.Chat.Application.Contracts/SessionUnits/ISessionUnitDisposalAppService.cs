using System.Threading.Tasks;
using IczpNet.Chat.SessionUnits.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.SessionUnits;

/// <summary>
/// 会话处置增量拉取服务接口
/// </summary>
public interface ISessionUnitDisposalAppService : IApplicationService
{
    /// <summary>
    /// 增量拉取会话处置记录（Remove/Delete/Hide/ClearMessage）
    /// 仅拉取 Id > input.AfterId 的增量记录
    /// </summary>
    Task<SessionUnitDisposalPagedResultDto> GetListAsync(SessionUnitDisposalGetListInput input);
}
