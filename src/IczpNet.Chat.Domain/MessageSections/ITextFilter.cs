using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections;

public interface ITextFilter
{
    Task CheckAsync(string text);

    Task<bool> IsContainsAsync(string text);
}
