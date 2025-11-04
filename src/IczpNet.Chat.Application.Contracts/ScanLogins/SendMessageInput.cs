using System.Collections.Generic;

namespace IczpNet.Chat.ScanLogins;

public class SendMessageInput
{

    public List<string> ConnectionIdList { get; set; }

    public LoginCommandPayload CommandPayload { get; set; }
}
