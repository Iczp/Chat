using IczpNet.Chat.BaseDtos;
using System;
namespace IczpNet.Chat.SessionBoxes;

///<summary>
/// 查询列表 
///</summary>
[Serializable]
public class BoxGetListInput : GetListInput
{
    /// <summary>
    /// 所属人
    /// </summary>
    public virtual long? OwnerId { get; set; }
}