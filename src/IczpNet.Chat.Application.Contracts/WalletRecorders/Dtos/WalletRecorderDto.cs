using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.WalletRecorders.Dtos
{
    public class WalletRecorderDto : BaseDto<Guid>
    {

        public virtual long AutoId { get; set; }

        public virtual long? OwnerId { get; set; }

        public virtual Guid? WalletId { get; set; }

        public virtual string BusinessId { get; set; }

        public virtual WalletBusinessTypes BusinessType { get; set; }

        public virtual decimal AvailableAmountBeforeChange { get; set; }

        public virtual decimal TotalAmountBeforeChange { get; set; }

        public virtual decimal LockAmountBeforeChange { get; set; }

        public virtual decimal AvailableAmount { get; set; }

        public virtual decimal TotalAmount { get; set; }

        public virtual decimal LockAmount { get; set; }

        public virtual decimal Amount { get; set; }

        public virtual string Description { get; set; }
    }
}
