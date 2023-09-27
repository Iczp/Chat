using IczpNet.Chat.InvitationCodes.Dtos;
using System;

namespace IczpNet.Chat.InvitationCodes;

public interface IInvitationCodeAppService :
    ICrudChatAppService<
        InvitationCodeDetailDto,
        InvitationCodeDto,
        Guid,
        InvitationCodeGetListInput,
        InvitationCodeCreateInput,
        InvitationCodeUpdateInput>
{
}
