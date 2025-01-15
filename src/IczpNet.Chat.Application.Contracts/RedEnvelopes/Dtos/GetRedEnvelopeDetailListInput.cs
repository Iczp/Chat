using IczpNet.Chat.BaseDtos;
using System;

namespace IczpNet.Chat.RedEnvelopes.Dtos;

/// <summary>
/// GetRedEnvelopeDetailListInput
/// </summary>
public class GetRedEnvelopeDetailListInput : GetListInput
{
    /// <summary>
    /// 红包d
    /// </summary>
    public Guid RedEnvelopeContentId { get; set; }
}
