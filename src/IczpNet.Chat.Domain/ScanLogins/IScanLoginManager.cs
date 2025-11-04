using IczpNet.Chat.ScanLogins;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.ScanLogins;

public interface IScanLoginManager
{
    Task<GenerateInfo> GenerateAsync(string connectionId);

    Task<GenerateInfo> ScanAsync(string scanText);

    Task<GrantedInfo> GrantAsync(string scanText);

    Task<RejectInfo> RejectAsync(string scanText, string reason);

    Task<GrantedInfo> GetGrantedInfoAsync(Guid loginCode);

    Task DeleteGrantedInfoAsync(Guid loginCode);
}
