using System;
using System.Collections.Generic;
using System.Text;

namespace IczpNet.Chat.Wallets.Dtos
{
    public class RechargeInput
    {
        public virtual Guid OwnerId { get; set; }

        public virtual decimal Amount { get; set; }

        public virtual string Password { get; set; }
        public string ConcurrencyStamp { get; set; }
        public string Description { get; set; }
    }
}
