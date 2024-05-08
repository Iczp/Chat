namespace IczpNet.Chat.Connections;

public class ConnectionOptions
{
    public int InactiveSeconds { get; set; } = 120;
    public int TimerPeriodSeconds { get; set; } = 10;
}
