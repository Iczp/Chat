using System;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Ulids;

public class UlidGenerator : IUlidGenerator, ITransientDependency
{
    public string Generate()
    {
        return Ulid.NewUlid().ToString();
    }
}
