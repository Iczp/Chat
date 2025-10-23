using IczpNet.Chat.ConnectionPools;
using Microsoft.AspNetCore.SignalR;

namespace IczpNet.Chat.ChatHubs;

public class CallerContext
{
    public HubCallerContext Context { get;  set; }

    public ConnectedEto Connect { get; set; }

    public CallerContext()
    {

    }

    public CallerContext(HubCallerContext context, ConnectedEto connect)
    {
        Context = context;
        Connect = connect;
    }
}
