using IczpNet.Chat.BaseDtos;
using System;
namespace IczpNet.Chat.SessionBoxes;

///<summary>
/// Dto 
///</summary>
[Serializable]
public class BoxCountDto
{
    /// <summary>
    /// Id
    /// </summary>
    public virtual Guid Id { get; set; }
    /// <summary>
    /// 名称
    /// </summary>

    public virtual string Name { get; set; }

    /// <summary>
    /// 所属性人
    /// </summary>
    public virtual long? OwnerId { get; set; }

    /// <summary>
    /// 消息数量
    /// </summary>
    public virtual long Count { get; set; }

}