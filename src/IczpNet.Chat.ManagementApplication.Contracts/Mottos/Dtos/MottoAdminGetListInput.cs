using IczpNet.Chat.Management.BaseDtos;

namespace IczpNet.Chat.Management.Mottos.Dtos;

public class MottoAdminGetListInput : BaseGetListInput
{
    public long? OwnerId { get; set; }
}
