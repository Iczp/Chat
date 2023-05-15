using System;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class KeywordOwnerSessionUnitSpecification : Specification<SessionUnit>
    {
        public virtual string Keyword { get; }
        public virtual IQueryable<long> DestinationIdList { get; }

        //public KeywordOwnerSessionUnitSpecification(string keyword)
        //{
        //    Keyword = keyword;
        //}

        public KeywordOwnerSessionUnitSpecification(string keyword, IQueryable<long> destinationIdList)
        {
            Keyword = keyword;
            DestinationIdList = destinationIdList;
        }

        public override Expression<Func<SessionUnit, bool>> ToExpression()
        {
            var expression = PredicateBuilder.New<SessionUnit>(x => x.MemberName.Contains(Keyword) || x.MemberNameSpellingAbbreviation.Contains(Keyword));

            if (DestinationIdList != null)
            {
                expression = expression.Or(x => DestinationIdList.Contains(x.OwnerId));
            }
            else
            {
                expression = expression.Or(x => x.Owner.Name.Contains(Keyword) || x.Owner.NameSpellingAbbreviation.Contains(Keyword));
            }

            return expression;
        }
    }
}
