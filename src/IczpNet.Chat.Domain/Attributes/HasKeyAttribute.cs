using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;

namespace IczpNet.Chat.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class HasKeyAttribute : Attribute
{
    public IReadOnlyList<string> PropertyNames { get; }

    public HasKeyAttribute(params string[] propertyNames)
    {
        Check.NotNullOrEmpty(propertyNames, nameof(propertyNames));
        PropertyNames = propertyNames.ToList();
    }
}
