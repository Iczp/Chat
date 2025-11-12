using System.Threading.Tasks;

namespace IczpNet.Chat.ScanCodes;

public interface IScanCodeManager
{

    Task<ScanCode> ScanCodeAsync(string type, string code);
}
