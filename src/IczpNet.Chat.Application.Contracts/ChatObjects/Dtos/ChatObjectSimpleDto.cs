using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.ChatObjects.Dtos
{
    public class ChatObjectSimpleDto : ChatObjectInfo, IEntityDto<long>
    {
        /// <summary>
        /// 设置加群、加好友、加聊天广场验证方式
        /// </summary>
        public virtual VerificationMethods VerificationMethod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string VerificationMethodDescription { get; set; }
    }
}
