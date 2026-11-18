using IczpNet.Chat.SessionUnits.Dtos;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageQuoteFastDto : MessageSimpleDto, IEntityDto<long>
{
    ///// <summary>
    ///// 发送人
    ///// </summary>
    public virtual SessionUnitMemberSenderDto SenderSessionUnit { get; set; }
}
