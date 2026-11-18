using System.Threading.Tasks;

namespace IczpNet.Chat.ShortIds;

public interface IShortIdGenerator
{
    Task<string> CreateAsync();
    string Create();
}
