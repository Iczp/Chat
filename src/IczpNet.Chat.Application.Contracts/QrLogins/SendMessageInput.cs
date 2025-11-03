using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.QrLogins;

public class SendMessageInput
{

    public List<string> ConnectionIdList { get; set; }

    public LoginCommandPayload CommandPayload { get; set; }
}
