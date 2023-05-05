using System;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class KeywordSessionUnitSpecification : Specification<SessionUnit>
    {
        public virtual string Keyword { get; }
        public KeywordSessionUnitSpecification(string keyword)
        {
            Keyword = keyword;
        }

        public override Expression<Func<SessionUnit, bool>> ToExpression()
        {
            return x => x.Rename.Contains(Keyword) || x.RenameSpellingAbbreviation.Contains(Keyword) || x.Owner.Name.Contains(Keyword) || x.Owner.NameSpellingAbbreviation.Contains(Keyword)
            ;
        }
    }
}
