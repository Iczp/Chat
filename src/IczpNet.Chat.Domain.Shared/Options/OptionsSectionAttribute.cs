using System;

namespace IczpNet.Chat.Options;

[AttributeUsage(AttributeTargets.Class)]
public sealed class OptionsSectionAttribute(string sectionKey) : Attribute
{
    public string SectionKey { get; } = sectionKey;
}