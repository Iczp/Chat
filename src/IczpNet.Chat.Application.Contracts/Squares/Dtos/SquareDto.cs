using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.Squares.Dtos
{
    public class SquareDto 
    {
        public virtual long Id { get; set; }

        //public virtual long? ParentId { get; set; }

        public virtual string Name { get; set; }

        public virtual string Portrait { get; set; }

        public virtual Guid? AppUserId { get; set; }

        public virtual ChatObjectTypeEnums? ObjectType { get; set; }

        public virtual string Description { get; set; }
    }
}
