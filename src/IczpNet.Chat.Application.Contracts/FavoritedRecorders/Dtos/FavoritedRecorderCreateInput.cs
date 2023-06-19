using System;

namespace IczpNet.Chat.FavoritedRecorders.Dtos
{
    public class FavoritedRecorderCreateInput
    {
        /// <summary>
        /// 会话单元Id
        /// </summary>
        public virtual Guid SessionUnitId { get; set; }

        public virtual long MessageId { get; set; }

        public virtual string DeviceId { get; set; }
    }
}
