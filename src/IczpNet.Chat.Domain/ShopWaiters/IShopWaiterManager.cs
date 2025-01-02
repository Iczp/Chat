using IczpNet.Chat.ChatObjects;
using System.Threading.Tasks;

namespace IczpNet.Chat.ShopWaiters;

public interface IShopWaiterManager
{

    Task<ChatObject> CreateAsync(long shopKeeperId, string name);

    Task<ChatObject> UpdateAsync(long id, string name);
}
