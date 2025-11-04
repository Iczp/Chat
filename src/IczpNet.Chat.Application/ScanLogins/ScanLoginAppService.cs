using IczpNet.Chat.BaseAppServices;

namespace IczpNet.Chat.ScanLogins;

public class ScanLoginAppService(IScanLoginManager scanLoginManager) : ChatAppService, IScanLoginAppService
{
    public IScanLoginManager ScanLoginManager { get; } = scanLoginManager;



}
