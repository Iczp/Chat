using IczpNet.AbpCommons.Dtos;

namespace IczpNet.Chat.ConnectionPools.Dtos;

public class LatestOnlineGetListInput : GetListInput
{
    public string DeviceId { get; set; }
    public string DeviceType { get; set; }
}
