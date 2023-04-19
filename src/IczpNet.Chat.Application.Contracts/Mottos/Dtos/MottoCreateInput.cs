namespace IczpNet.Chat.Mottos.Dtos;

public class MottoCreateInput : MottoUpdateInput
{
    public virtual long OwnerId { get; set; }
}
