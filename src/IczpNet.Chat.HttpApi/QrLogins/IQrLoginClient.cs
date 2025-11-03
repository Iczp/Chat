using IczpNet.Chat.CommandPayloads;
using System.Threading.Tasks;

namespace IczpNet.Chat.QrLogins;

public interface IQrLoginClient
{
    Task ReceivedMessage(LoginCommandPayload pushPayload);
}
