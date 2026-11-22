using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.Entries.Dtos;
using System.Collections.Generic;

namespace IczpNet.Chat.SessionUnits.Dtos;

public class SessionUnitDetailDto : SessionUnitOwnerDto
{

    /// <summary>
    /// 
    /// </summary>
    public virtual ChatObjectDto Owner { get; set; }

    /// <summary>
    /// 传话单元数量
    /// </summary>
    public virtual double SessionUnitCount { get; set; }

    /// <summary>
    /// 会话单元条目列表（自定义扩展属性）
    /// </summary>
    public virtual List<EntryObjectDto> Entries { get; set; }

}
