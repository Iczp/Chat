using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.Mottos.Dtos;

public class MottoAdminGetListInput : GetListInput
{
    public long? OwnerId { get; set; }
}
