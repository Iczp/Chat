using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Management.ChatObjects.Dtos
{
    public class ChatObjectSimpleDto : ChatObjectInfo, IEntityDto<long>
    {
        public virtual VerificationMethods VerificationMethod { get; set; }
    }
}
