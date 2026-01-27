using IczpNet.AbpCommons.Dtos;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class LastOnlineGetListInput : GetListInput
{
    public string DeviceId { get; set; }
    public string DeviceType { get; set; }
}
