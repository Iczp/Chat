using IczpNet.Chat.Enums;
using System;

namespace IczpNet.Chat.Ai;

public interface IAiResolver
{
    bool HasProvider(string name);

    Type GetProvider(string name);

    Type GetProviderOrDefault(string name);
}
