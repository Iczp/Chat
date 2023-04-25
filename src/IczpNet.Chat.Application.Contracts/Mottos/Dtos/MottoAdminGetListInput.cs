using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.Mottos.Dtos;

public class MottoAdminGetListInput : BaseGetListInput
{
    public long? OwnerId { get; set; }
}
