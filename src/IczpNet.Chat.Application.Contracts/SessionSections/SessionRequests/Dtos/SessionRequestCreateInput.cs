using IczpNet.Chat.BaseDtos;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.SessionSections.SessionRequests.Dtos;

public class SessionRequestCreateInput : BaseInput
{
    /// <summary>
    /// 所属聊天对象Id[发起者]
    /// </summary>
    [Required]
    public virtual long OwnerId { get; set; }

    /// <summary>
    /// 目标聊天对象Id[被请求者]
    /// </summary>
    [Required] 
    public virtual long DestinationId { get; set; }

    /// <summary>
    /// 请求消息
    /// </summary>
    public virtual string RequestMessage { get; set; }

}
