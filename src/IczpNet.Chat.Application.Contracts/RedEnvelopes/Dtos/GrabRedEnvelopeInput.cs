using System;

namespace IczpNet.Chat.RedEnvelopes.Dtos;

/// <summary>
/// GrabRedEnvelopeInput
/// </summary>
public class GrabRedEnvelopeInput
{
    /// <summary>
    /// 红包Id
    /// </summary>
    public Guid RedEnvelopeContentId { get; set; }
    /// <summary>
    /// 消息Id
    /// </summary>
    public Guid MessageId { get; set; }
}
