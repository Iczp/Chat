using IczpNet.Chat.SessionUnits.Dtos;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.MessageSections.Messages.Dtos;

public class MessageQuoteDto : MessageDto, IEntityDto<long>
{
    /// <summary>
    /// 发送人显示名称
    /// </summary>
    public virtual string SenderDisplayName { get; set; }

    ///// <summary>
    ///// 发送人
    ///// </summary>
    public override SessionUnitSenderDto SenderSessionUnit { get; set; }


}
