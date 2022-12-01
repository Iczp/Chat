using FluentValidation;
using System;
using System.Collections.Generic;

namespace IczpNet.Chat.Validators
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

        
    }
}
