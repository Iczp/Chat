using FluentValidation;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Validators;

public static class ValidatorExtentions
{
    public readonly static string[] UriSchemes = new[] { Uri.UriSchemeHttp, Uri.UriSchemeHttps };

    public static IRuleBuilderOptions<T, IList<TElement>> MaxCount<T, TElement>(this IRuleBuilder<T, IList<TElement>> ruleBuilder, int maxCount)
    {

        return ruleBuilder.Must((rootObject, list, context) =>
        {
            context.MessageFormatter
              .AppendArgument("MaxElements", maxCount)
              .AppendArgument("TotalElements", list.Count);
            return list.Count < maxCount;
        })
        .WithMessage("{PropertyName} must contain fewer than {MaxElements} items. The list contains {TotalElements} element");
    }

    public static IRuleBuilderOptions<T, IList<TElement>> MinCount<T, TElement>(this IRuleBuilder<T, IList<TElement>> ruleBuilder, int minCount)
    {
        return ruleBuilder.Must((rootObject, list, context) =>
        {
            context.MessageFormatter
              .AppendArgument("MaxElements", minCount)
              .AppendArgument("TotalElements", list.Count);
            return list.Count > minCount;
        })
        .WithMessage("{PropertyName} must contain fewer than {MaxElements} items. The list contains {TotalElements} element");
    }

    public static IRuleBuilderOptions<T, string> Url<T>(this IRuleBuilder<T, string> ruleBuilder, UriKind uriKind = UriKind.RelativeOrAbsolute)
    {
        return ruleBuilder.SetValidator(new UrlValidator<T>(uriKind))
            .WithMessage("'{PropertyName}' is fail(Url):{PropertyValue}");
    }
}
