using System.Threading.Tasks;

namespace IczpNet.Chat.WalletOrders;

public interface IWalletOrderNoGenerator
{
    Task<string> MakeAsync();
}
