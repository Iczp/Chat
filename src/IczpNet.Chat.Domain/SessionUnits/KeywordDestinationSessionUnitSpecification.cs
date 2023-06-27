using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;
using IczpNet.AbpCommons.Extensions;

namespace IczpNet.Chat.SessionUnits
{
    public class KeywordDestinationSessionUnitSpecification : Specification<SessionUnit>
    {
        public virtual string Keyword { get; }

        public virtual IEnumerable<long> OwnerIdList { get; }

        //public KeywordDestinationSessionUnitSpecification(string keyword)
        //{
        //    Keyword = keyword;
        //}

        public KeywordDestinationSessionUnitSpecification(string keyword, IEnumerable<long> ownerIdList)
        {
            Keyword = keyword;
            OwnerIdList = ownerIdList;
        }

        public override Expression<Func<SessionUnit, bool>> ToExpression()
        {
            var expression = PredicateBuilder.New<SessionUnit>();

            expression = expression.Or(x => x.Setting.Rename.IndexOf(Keyword) == 0);
            expression = expression.Or(x => x.Setting.RenameSpellingAbbreviation.IndexOf(Keyword) == 0);

            //Write diffusion
            expression = expression.Or(x => x.Destination.Name.IndexOf(Keyword) == 0);
            expression = expression.Or(x => x.Destination.NameSpellingAbbreviation.IndexOf(Keyword) == 0);

            if (OwnerIdList.IsAny())
            {
                expression = expression.Or(x => OwnerIdList.Contains(x.DestinationId.Value));
            }

            return expression;
        }
    }
}
