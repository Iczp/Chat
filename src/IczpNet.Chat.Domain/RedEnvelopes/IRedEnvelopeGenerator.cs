using IczpNet.Chat.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.RedEnvelopes;

public interface IRedEnvelopeGenerator
{
    Task<IList<RedEnvelopeUnit>> MakeAsync(GrantModes grantMode, Guid redEnvelopeContentId, decimal amount, int count, decimal totalAmount);
}
