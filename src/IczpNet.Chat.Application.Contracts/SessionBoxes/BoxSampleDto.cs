using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.SessionBoxes;

/// <summary>
/// SampleDto
/// </summary>
[Serializable]
public class BoxSampleDto : BaseDto<Guid>
{
    /// <summary>
    /// 名称
    /// </summary>

    public virtual string Name { get; set; }

    /// <summary>
    /// 所属性人
    /// </summary>
    public virtual long? OwnerId { get; set; }

}