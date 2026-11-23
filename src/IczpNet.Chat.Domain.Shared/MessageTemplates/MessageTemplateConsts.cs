namespace IczpNet.Chat.MessageTemplates;

public static class MessageTemplateConsts
{
    public const int SchemaVersion = 1;

    public const int MaxNameLength = 80;
    public const int MaxDescriptionLength = 256;
    public const int MaxFallbackTextLength = 256;

    public const int MaxNodes = 50;
    public const int MaxDepth = 6;
    public const int MaxFields = 30;
    public const int MaxActions = 3;
    public const int MaxResources = 3;

    public const int MaxStaticTextLength = 1000;
    public const int MaxFieldTextLength = 2000;

    public const int MaxDefinitionBytes = 32 * 1024; // 32 KiB
    public const int MaxSnapshotBytes = 64 * 1024;   // 64 KiB
}
