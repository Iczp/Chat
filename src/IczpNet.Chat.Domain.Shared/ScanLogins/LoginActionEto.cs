using System;

namespace IczpNet.Chat.ScanLogins;

public class LoginActionEto
{
    public string Action {  get; set; }

    public string Description { get; set; }

    public string UserName { get; set; }

    public Guid? UserId { get; set; }

    public string CliendId { get; set; }

    public string LoginCode { get; set; }
}
