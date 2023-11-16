using IczpNet.Chat.SessionUnits.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.SessionUnits;

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
    /// 保存到通讯录
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isContacts"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> SetIsContactsAsync(Guid sessionUnitId, bool isContacts);

    /// <summary>
    /// 是否显示成员名称
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="isShowMemberName"></param>
    /// <returns></returns>
    Task<SessionUnitOwnerDto> SetIsShowMemberNameAsync(Guid sessionUnitId, bool isShowMemberName);

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

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task DeleteMessageAsync(Guid sessionUnitId, long messageId);

    /// <summary>
    /// 设置联系人标签
    /// </summary>
    /// <param name="sessionUnitId"></param>
    /// <param name="ContactTagIdList"></param>
    /// <returns></returns>
    Task SetContactTagsAsync(Guid sessionUnitId, List<Guid> ContactTagIdList);

    /// <summary>
    /// 禁言过期时间，为空则不禁言
    /// </summary>
    /// <param name="muterSessionUnitId">被设置的会话单元Id</param>
    /// <param name="setterSessionUnitId">设置者会话单元Id</param>
    /// <param name="seconds">禁言(秒)，为0则取消禁言</param>
    /// <returns>禁言过期时间</returns>
    Task<DateTime?> SetMuteExpireTimeAsync(Guid muterSessionUnitId, Guid setterSessionUnitId, int seconds);
}
