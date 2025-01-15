namespace IczpNet.Chat.RedEnvelopes.Dtos;

public class GrabRedEnvelopeOutput
{
    /// <summary>
    /// 是否抢到
    /// </summary>
    public bool IsGrabed { get; set; }
    /// <summary>
    /// 文本
    /// </summary>
    public string Message { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public RedEnvelopeDetailResult Detail { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public GrabRedEnvelopeOutput() { }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="isGrabed">是否抢到</param>
    /// <param name="message">Message</param>
    /// <param name="detail">Detail</param>
    public GrabRedEnvelopeOutput(bool isGrabed, string message, RedEnvelopeDetailResult detail = null)
    {

        IsGrabed = isGrabed;
        Message = message;
        Detail = detail;
    }
}
