using System;
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

    /// <summary>
    /// 退出群聊（Remove: 离开群聊，保留已有历史聊天记录，不再接收新消息）
    /// </summary>
    Task<SessionUnitDisposalDto> RemoveAsync(Guid sessionUnitId, string reason = null);

    /// <summary>
    /// 退出群聊并软删除会话（QuitAndDeleted: 退出群聊且从会话列表隐藏/删除历史）
    /// </summary>
    Task<SessionUnitDisposalDto> QuitAndDeleteAsync(Guid sessionUnitId);

    /// <summary>
    /// 获取指定会话单元的最新处置状态
    /// </summary>
    Task<SessionUnitDisposalDto> GetStatusAsync(Guid sessionUnitId);
}
