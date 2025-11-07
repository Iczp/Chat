using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.ScanLogins;

public interface IScanLoginManager
{
    Task<GenerateInfo> GenerateAsync(string connectionId, string state);

    Task<GenerateInfo> ScanAsync(string scanText);

    Task<GrantedInfo> GrantAsync(string scanText);

    Task<RejectInfo> RejectAsync(string scanText, string reason);

    Task<CancelInfo> CancelAsync(string connectionId, string reason);

    Task<GrantedInfo> GetGrantedInfoAsync(Guid scanToken);

    Task RemoveAsync(string connectionId);

    Task DeleteGrantedInfoAsync(Guid scanToken);
}
