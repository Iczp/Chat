using System;

namespace IczpNet.Chat.hubs;

public interface IChatClient
{
   void FetchNewMessage(string sessionUnitId);
}
