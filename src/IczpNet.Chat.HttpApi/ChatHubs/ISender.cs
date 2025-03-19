using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.ChatHubs;

public interface ISender
{
    Task SendAsync(string method, object payload);
}
