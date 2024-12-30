using IczpNet.Chat.Enums;
using System.Threading.Tasks;

namespace IczpNet.Chat.RedEnvelopes;

public interface IRedPacketManager
{
    /// <summary>
    /// 创建红包
    /// </summary>
    /// <param name="ownerId"></param>
    /// <param name="grantMode"></param>
    /// <param name="amount"></param>
    /// <param name="count"></param>
    /// <param name="totalAmount"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    Task<RedEnvelopeContent> CreateAsync(long ownerId, GrantModes grantMode, decimal amount, int count, decimal totalAmount, string text);
}
