namespace IczpNet.Chat.Management.Mottos.Dtos;

public class MottoAdminCreateInput : MottoUpdateInput
{
    public virtual long OwnerId { get; set; }
}
