using IczpNet.Chat.CommandPayloads;
using System.Threading.Tasks;

namespace IczpNet.Chat.ScanLogins;

public interface IScanLoginClient
{
    Task ReceivedMessage(LoginCommandPayload pushPayload);
}
