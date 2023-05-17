using System;

namespace IczpNet.Chat.Favorites.Dtos
{
    public class FavoriteCreateInput
    {
        public Guid SessionUnitId { get; set; }

        public long MessageId { get; set; }

        public string DeviceId { get; set; }
    }
}
