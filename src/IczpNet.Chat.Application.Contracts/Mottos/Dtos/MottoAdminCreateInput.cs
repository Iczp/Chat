namespace IczpNet.Chat.Mottos.Dtos;

public class MottoAdminCreateInput : MottoUpdateInput
{
    public virtual long OwnerId { get; set; }
}
