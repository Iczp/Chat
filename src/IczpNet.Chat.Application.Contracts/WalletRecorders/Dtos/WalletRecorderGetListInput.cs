using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.WalletRecorders.Dtos
{
    public class WalletRecorderGetListInput : BaseGetListInput
    {
        public virtual long? OwnerId { get; set; }

        public virtual string BusinessId { get; set; }

        public virtual decimal? MinAmount { get; set; }

        public virtual decimal? MaxAmount { get; set; }
    }
}
