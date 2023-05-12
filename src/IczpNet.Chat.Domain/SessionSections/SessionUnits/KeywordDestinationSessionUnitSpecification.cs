using System;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class KeywordDestinationSessionUnitSpecification : Specification<SessionUnit>
    {
        public virtual string Keyword { get; }

        public virtual IQueryable<long> OwnerIdList { get; }

        //public KeywordDestinationSessionUnitSpecification(string keyword)
        //{
        //    Keyword = keyword;
        //}

        public KeywordDestinationSessionUnitSpecification(string keyword, IQueryable<long> ownerIdList)
        {
            Keyword = keyword;
            OwnerIdList = ownerIdList;
        }

        public override Expression<Func<SessionUnit, bool>> ToExpression()
        {
            var expression = PredicateBuilder.New<SessionUnit>(x => x.Rename.Contains(Keyword) || x.RenameSpellingAbbreviation.Contains(Keyword));

            if (OwnerIdList != null)
            {
                expression = expression.Or(x => OwnerIdList.Contains(x.DestinationId.Value));
            }
            else
            {
                expression = expression.Or(x => x.Destination.Name.Contains(Keyword) || x.Destination.NameSpellingAbbreviation.Contains(Keyword));
            }

            return expression;
        }
    }
}
