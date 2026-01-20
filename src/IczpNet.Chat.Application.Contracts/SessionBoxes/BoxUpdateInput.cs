using System; 
namespace IczpNet.Chat.SessionBoxes; 

///<summary>
/// 修改 
///</summary>
[Serializable] 
public class BoxUpdateInput 
{
    /// <summary>
    /// 名称
    /// </summary>

    public virtual string Name { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    public virtual string Description { get; set; }


}