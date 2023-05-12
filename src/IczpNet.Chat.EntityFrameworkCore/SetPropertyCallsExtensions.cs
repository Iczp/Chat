using IczpNet.Chat.SessionSections.SessionUnits;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using System;

namespace IczpNet.Chat
{
    public static class SetPropertyCallsExtensions
    {
        public static SetPropertyCalls<TSource> SetPropertyIf<TSource,TProperty>(
            [NotNull] this SetPropertyCalls<TSource> setPropertyCalls,
            bool ifTrue,
            Func<TSource, TProperty> propertyExpression,
            Func<TSource, TProperty> valueExpression)
            where TSource : class
        {
            if (ifTrue)
            {
                return setPropertyCalls.SetProperty(propertyExpression, valueExpression);
            }
            return setPropertyCalls;
        }
    }
}
