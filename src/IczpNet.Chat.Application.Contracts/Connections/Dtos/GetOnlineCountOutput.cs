using System;

namespace IczpNet.Chat.Connections.Dtos
{
    public class GetOnlineCountOutput
    {
        public DateTime CurrentTime { get; set; }
        public int Count { get; set; }
    }
}