using System.Threading.Tasks;

namespace IczpNet.Chat.QrLogins;

public interface IQrLoginManager
{
    Task<GenerateInfo> GenerateAsync(string connectionId);

    Task<GenerateInfo> ScanAsync(string qrText);

    Task<GrantedInfo> GrantAsync(string qrText);

    Task<RejectInfo> RejectAsync(string qrText);
}
