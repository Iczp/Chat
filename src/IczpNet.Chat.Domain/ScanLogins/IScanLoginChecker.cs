using System.Threading.Tasks;

namespace IczpNet.Chat.ScanLogins;

public interface IScanLoginChecker
{

    Task CheckAsync(string scanText);
}
