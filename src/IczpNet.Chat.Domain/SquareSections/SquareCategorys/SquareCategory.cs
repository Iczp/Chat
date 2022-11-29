using IczpNet.Chat.BaseEntitys;
using IczpNet.Chat.SquareSections.Squares;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.SquareSections.SquareCategorys
{
    public class SquareCategory : BaseTreeEntity<SquareCategory, Guid>
    {

        public virtual IList<Square> SquareList { get; set; }
    }
}
