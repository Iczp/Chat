using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitCacheGetListInput : GetListInput
{
    /// <summary>
    /// 会话Id（二者不能同时为null）
    /// </summary>
    public Guid? SessionId { get; set; }

    /// <summary>
    /// 会话单元Id（二者不能同时为null）
    /// </summary>
    public Guid? SessionUnitId { get; set; }
}
