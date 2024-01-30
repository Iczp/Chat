using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectUpdateInput : BaseTreeInputDto<long>
{
    /// <summary>
    /// 设置加群、加好友、加聊天广场验证方式
    /// </summary>
    public virtual VerificationMethods VerificationMethod { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual Genders Gender { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public virtual string Description { get; set; }


}
