using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.InvitationCodes.Dtos;

public class InvitationCodeUpdateInput : BaseInput
{
    public virtual string Title { get; set; }
}
