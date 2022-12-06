using System;
using System.ComponentModel;

namespace IczpNet.Chat.MessageSections
{
    public abstract class BaseMessageContentInfo
    {
        [DefaultValue(null)]
        public virtual Guid? Id { get; set; }
    }
}
