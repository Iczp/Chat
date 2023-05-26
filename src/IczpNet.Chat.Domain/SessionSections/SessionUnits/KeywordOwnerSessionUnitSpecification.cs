using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;
using IczpNet.AbpCommons.Extensions;

namespace IczpNet.Chat.SessionSections.SessionUnits
{
    public class KeywordOwnerSessionUnitSpecification : Specification<SessionUnit>
    {
        public virtual string Keyword { get; }
        public virtual IEnumerable<long> DestinationIdList { get; }

        //public KeywordOwnerSessionUnitSpecification(string keyword)
        //{
        //    Keyword = keyword;
        //}

        public KeywordOwnerSessionUnitSpecification(string keyword, IEnumerable<long> destinationIdList)
        {
            Keyword = keyword;
            DestinationIdList = destinationIdList;
        }

        public override Expression<Func<SessionUnit, bool>> ToExpression()
        {
            var expression = PredicateBuilder.New<SessionUnit>();

            expression = expression.Or(x => x.Setting.MemberName.IndexOf(Keyword) == 0);
            expression = expression.Or(x => x.Setting.MemberNameSpellingAbbreviation.IndexOf(Keyword) == 0);

            //Write diffusion
            expression = expression.Or(x => x.Owner.Name.IndexOf(Keyword)==0);
            expression = expression.Or(x => x.Owner.NameSpellingAbbreviation.IndexOf(Keyword) == 0);

            if (DestinationIdList.IsAny())
            {
                expression = expression.Or(x => DestinationIdList.Contains(x.OwnerId));
            }

            return expression;
        }
    }
}
