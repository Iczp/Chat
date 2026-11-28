namespace IczpNet.Chat.Calls;

public class LiveKitOptions
{
    public const string SectionName = "LiveKit";

    public string InternalUrl { get; set; } = "http://livekit:7880";
    public string PublicWsUrl { get; set; } = "ws://10.0.5.20:7880";
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    public string IceTransportPolicy { get; set; } = "all";
    public int MaxGroupParticipants { get; set; } = CallConsts.MaxGroupParticipants;
}
