using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.RedEnvelopes
{
    public interface IRedEnvelopeGenerator
    {
        Task<IList<RedEnvelopeUnit>> MakeAsync(GrantModes grantMode, decimal amount, int count, decimal totalAmount, string text);
    }
}
