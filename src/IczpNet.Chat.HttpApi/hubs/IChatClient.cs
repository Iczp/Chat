using System;

namespace IczpNet.Chat.Hubs;

public interface IChatClient
{
   void FetchNewMessage(string sessionUnitId);
}
