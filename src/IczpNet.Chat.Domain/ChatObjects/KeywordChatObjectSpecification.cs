using System;
using System.Linq;
using System.Linq.Expressions;
using Volo.Abp.Specifications;

namespace IczpNet.Chat.ChatObjects;

public class KeywordChatObjectSpecification : Specification<ChatObject>
{
    public virtual string Keyword { get; }

    public KeywordChatObjectSpecification(string keyword)
    {
        Keyword = keyword;
    }



    public override Expression<Func<ChatObject, bool>> ToExpression()
    {
        var expression = PredicateBuilder.New<ChatObject>();

        //Write diffusion
        expression = expression.Or(x => x.Name.Contains(Keyword));
        expression = expression.Or(x => x.NameSpellingAbbreviation.IndexOf(Keyword) == 0);

        return expression;
    }
}
