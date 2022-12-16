using System;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionDto
    {
        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual int Badge { get; set; }

        /// <summary>
        /// Set top
        /// </summary>
        public virtual double Sorting { get; set; }

        /// <summary>
        /// MaxAutoId
        /// </summary>
        public virtual long MessageAutoId { get; set; }

        public virtual Guid? MessageId { get; set; }

        public virtual DateTime ShowTime { get; set; }
    }
}
