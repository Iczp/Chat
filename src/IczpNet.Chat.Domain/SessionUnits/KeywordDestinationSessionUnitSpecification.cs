using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;
using IczpNet.AbpCommons.Extensions;

namespace IczpNet.Chat.SessionUnits;

public class KeywordDestinationSessionUnitSpecification(string keyword, IEnumerable<long> ownerIdList) : Specification<SessionUnit>
{
    public virtual string Keyword { get; } = keyword;

    public virtual IEnumerable<long> OwnerIdList { get; } = ownerIdList;

    public override Expression<Func<SessionUnit, bool>> ToExpression()
    {
        var expression = PredicateBuilder.New<SessionUnit>();

        expression = expression.Or(x => x.Setting.Rename.Contains(Keyword));
        expression = expression.Or(x => x.Setting.RenameSpellingAbbreviation.Contains(Keyword));

        //Write diffusion
        expression = expression.Or(x => x.Destination.Name.Contains(Keyword) );
        expression = expression.Or(x => x.Destination.NameSpellingAbbreviation.Contains(Keyword));

        if (OwnerIdList.IsAny())
        {
            expression = expression.Or(x => OwnerIdList.Contains(x.DestinationId.Value));
        }

        return expression;
    }
}
