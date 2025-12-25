using IczpNet.Chat.Options;

namespace IczpNet.Chat.MessageReports;

[OptionsSection("MessageReport")]
public class MessageReportOptions : IConventionOptions
{

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; } = true;
    /// <summary>
    /// 秒
    /// </summary>
    public int FlushToDbTimerPeriodSeconds { get; set; } = 60 * 5;
}