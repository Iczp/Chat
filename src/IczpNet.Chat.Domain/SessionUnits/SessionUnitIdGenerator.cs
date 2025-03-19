using IczpNet.Chat.TextTemplates;
using System;
using System.Text.RegularExpressions;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.SessionUnits;

public partial class SessionUnitIdGenerator : DomainService, ISessionUnitIdGenerator
{
    protected virtual string Separator { get; set; } = "x";
    protected virtual string DestinationInText { get; set; } = "ghijklmnopqrstuvwyzabcdef";
    protected virtual string OwnerInText { get; set; } = "fedcbazywvugtsrqponmlkjih";

    public virtual Guid Create(long ownerId, long destinationId)
    {
        return GuidGenerator.Create();
    }

    public virtual string Generate(long ownerId, long destinationId)
    {
        return GenerateOwnerString(ownerId) + Separator + GenerateDestinationString(destinationId);
    }

    public virtual long[] Resolving(string sessionUnitId)
    {
        var arr = sessionUnitId.Split(Separator);

        if (arr.Length == 2)
        {
            return new long[] { ResolvingOwnerString(arr[0]), ResolvingDestinationString(arr[1]) };
        }

        return Array.Empty<long>();
    }

    protected virtual string GenerateOwnerString(long ownerId)
    {
        return IntStringHelper.IntToString(ownerId, OwnerInText.Length, OwnerInText);
    }

    protected virtual string GenerateDestinationString(long destinationId)
    {
        return IntStringHelper.IntToString(destinationId, DestinationInText.Length, DestinationInText);
    }

    protected virtual long ResolvingOwnerString(string ownerString)
    {
        return IntStringHelper.StringToInt(ownerString, OwnerInText.Length, OwnerInText);
    }

    protected virtual long ResolvingDestinationString(string destinationString)
    {
        return IntStringHelper.StringToInt(destinationString, DestinationInText.Length, DestinationInText);
    }

    public virtual bool IsVerified(string sessionUnitId)
    {
        return SessionUnitIdRegex().IsMatch(sessionUnitId);
    }

    [GeneratedRegex(@"^[a-wyz]+x[a-wyz]+$")]
    private static partial Regex SessionUnitIdRegex();
}

