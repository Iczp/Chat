using IczpNet.Chat.BaseDtos;
using System; 
namespace IczpNet.Chat.SessionBoxes; 

///<summary>
/// Dto 
///</summary>
[Serializable] 
public class BoxCountDto : BaseDto<Guid>
{
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