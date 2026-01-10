namespace IczpNet.Chat.ConnectionPools;

public class DeviceModel
{
    public string ConnectionId { get; set; }
    public long OwnerId { get; set; }
    public string DeviceType { get; set; }
    public string DeviceId { get; set; }
    public override string ToString()
        => $"{nameof(ConnectionId)}={ConnectionId},{nameof(DeviceType)}={DeviceType},{nameof(DeviceId)}={DeviceId}";

}