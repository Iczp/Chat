using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System.Collections.Generic;

namespace IczpNet.Chat.OfficialSections.Officials.Dtos;

public class OfficialCreateInput : BaseInput
{
    public virtual string Name { get; set; }

    public virtual string Code { get; set; }

    /// <summary>
    /// 群拥有者 OwnerUserId (群主)
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 群类型（自由群、职位群）
    /// </summary>
    public virtual OfficialTypes Type { get; set; }

    public virtual string Description { get; set; }

    /// <summary>
    /// ChatObjectId
    /// </summary>
    public virtual List<long> ChatObjectIdList { get; set; }
}
