using System;

namespace IczpNet.Chat.FavoritedRecorders.Dtos
{
    public class FavoritedRecorderCreateInput
    {
        public Guid SessionUnitId { get; set; }

        public long MessageId { get; set; }

        public string DeviceId { get; set; }
    }
}
