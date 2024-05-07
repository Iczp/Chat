using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace IczpNet.Chat.Comparers;


public class EntityComparer<TSource> : EntityComparer<TSource, string>
{
    public EntityComparer(Func<TSource, string> projection) : base(projection)
    {
    }
}

public class EntityComparer<TSource, TKey> : EqualityComparer<TSource>, IEqualityComparer<TSource>
{
    public virtual Func<TSource, TKey> Projection { get; private set; }

    public override bool Equals(TSource x, TSource y)
    {
        if (x == null && y == null)
        {
            return true;
        }
        if (x == null || y == null)
        {
            return false;
        }

        return Projection(x).Equals(Projection(y));
    }

    public override int GetHashCode([DisallowNull] TSource obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException("obj");
        }
        //return obj.GetHashCode();
        return 1;
    }

    public EntityComparer(Func<TSource, TKey> projection)
    {
        Projection = projection;
    }
}
