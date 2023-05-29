using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Counters
{
    public abstract class MessageCounterArgs
    {
        public virtual List<long> MessageIdList { get; set;}
    }
}
