using System; 
namespace IczpNet.Chat.SessionBoxes; 

///<summary>
/// Dto 
///</summary>
[Serializable] 
public class BoxDto : BoxSampleDto 
{

    ///<summary>
    /// 说明 
    ///</summary>
    public virtual string Description { get; set; }

}