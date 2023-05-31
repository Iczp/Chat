using FluentValidation;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IczpNet.Chat.Validators
{
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

        public static IRuleBuilderOptions<T, TProperty> IsUrl<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, UriKind uriKind = UriKind.RelativeOrAbsolute)
        {
            return ruleBuilder.Must((x, val, context) =>
            {
                //context.MessageFormatter
                // .AppendArgument("MaxElements", minCount)
                // .AppendArgument("TotalElements", list.Count);

                return Uri.TryCreate(val.ToString(), uriKind, out Uri uriResult) && UriSchemes.Contains(uriResult.Scheme);
            })
            .WithMessage("{PropertyName} is fail(Url)"); 
        }
    }
}
