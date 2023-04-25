using FluentValidation;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Management.Validators
{
    public static class ValidatorExtentions
    {

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
    }
}
