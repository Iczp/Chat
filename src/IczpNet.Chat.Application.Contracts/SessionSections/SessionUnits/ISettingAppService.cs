using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionSections.SessionUnits;

public interface ISettingAppService
{
    /// <summary>
    /// 设置会话内名称
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="memberName"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> SetMemberNameAsync(Guid sessionUnitId, string memberName);

    /// <summary>
    /// 备注
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="rename"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> SetRenameAsync(Guid sessionUnitId, string rename);

    /// <summary>
    /// 置顶会话
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isTopping"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> SetToppingAsync(Guid sessionUnitId, bool isTopping);

    /// <summary>
    /// 设置为已读
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isForce"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> SetReadedMessageIdAsync(Guid sessionUnitId, bool isForce = false, long? messageId = null);

    /// <summary>
    /// 设置为免打扰
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isImmersed"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> SetImmersedAsync(Guid sessionUnitId, bool isImmersed);

    /// <summary>
    /// 退出聊天（主动）
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> RemoveAsync(Guid sessionUnitId);

    /// <summary>
    /// 退出聊天（主动）
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> ExitAsync(Guid sessionUnitId);

    /// <summary>
    /// 踢出聊天
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> KillAsync(Guid sessionUnitId);

    /// <summary>
    /// 清空消息
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> ClearMessageAsync(Guid sessionUnitId);

    Task<SessionUnitOwnerDto> DeleteMessageAsync(Guid sessionUnitId, long messageId);
}
