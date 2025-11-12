using System.Threading.Tasks;

namespace IczpNet.Chat.ScanCodes;

public interface IScanCodeHandler
{
    Task<ScanHandlerResult> HandleAsync(ScanCode scanCode);
}
