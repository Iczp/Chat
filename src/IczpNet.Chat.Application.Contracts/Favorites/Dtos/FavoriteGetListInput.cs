using IczpNet.Chat.BaseDtos;

namespace IczpNet.Chat.Favorites.Dtos
{
    public class FavoriteGetListInput : BaseGetListInput
    {
        public long? OwnerId { get; set; }

        public long? DestinationId { get; set; }
    }
}
