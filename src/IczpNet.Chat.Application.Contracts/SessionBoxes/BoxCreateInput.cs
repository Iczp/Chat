using System;
namespace IczpNet.Chat.SessionBoxes;

///<summary>
/// 新增 
///</summary>
[Serializable]
public class BoxCreateInput
{
    ///<summary>
    /// 名称 
    ///</summary>
    public virtual string Name { get; set; }

    ///<summary>
    /// 说明 
    ///</summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 所属性人
    /// </summary>
    public virtual long? OwnerId { get; set; }
}