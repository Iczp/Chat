using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

namespace IczpNet.Chat.Ulids;

public class UlidToStringConverter(ConverterMappingHints? mappingHints) : ValueConverter<Ulid, string>(
            convertToProviderExpression: x => x.ToString(),
            convertFromProviderExpression: x => Ulid.Parse(x),
            mappingHints: DefaultHints.With(mappingHints))
{
    private static readonly ConverterMappingHints DefaultHints = new(size: 26);

    public UlidToStringConverter() : this(null)
    {
    }
}