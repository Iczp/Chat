namespace IczpNet.Chat.InvitationCodes.Dtos;

public class InvitationCodeCreateInput : InvitationCodeUpdateInput
{
    public virtual long OwnerId { get; set; }
}
