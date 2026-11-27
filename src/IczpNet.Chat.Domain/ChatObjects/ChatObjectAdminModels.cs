using System;
using System.Collections.Generic;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.ChatObjects;

public class ChatObjectAdminQueryFilter
{
    public string Keyword { get; set; }

    public string Code { get; set; }

    public Guid? AppUserId { get; set; }

    public string ChatObjectTypeId { get; set; }

    public ChatObjectTypeEnums? ObjectType { get; set; }

    public List<ChatObjectTypeEnums> ObjectTypes { get; set; }

    public List<Guid> CategoryIdList { get; set; }

    public bool IncludeChildCategories { get; set; }

    public bool? IsEnabled { get; set; }

    public bool? IsDefault { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsStatic { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeveloper { get; set; }

    public string Sorting { get; set; }

    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 10;
}

public class ChatObjectAdminListResult
{
    public long TotalCount { get; set; }

    public List<ChatObject> Items { get; set; } = new();
}

public class ChatObjectAdminEntryValueModel
{
    public Guid EntryNameId { get; set; }

    public string EntryName { get; set; }

    public Guid EntryValueId { get; set; }

    public string Value { get; set; }
}

public class ChatObjectAdminDetailResult
{
    public ChatObject Entity { get; set; }

    public List<Guid> CategoryIds { get; set; } = new();

    public List<string> CategoryNames { get; set; } = new();

    public List<ChatObjectAdminEntryValueModel> EntryValues { get; set; } = new();

    public int SameUserChatObjectCount { get; set; }
}

public class ChatObjectAdminCreateArgs
{
    public string Name { get; set; }

    public string Code { get; set; }

    public string Description { get; set; }

    public string ChatObjectTypeId { get; set; }

    public ChatObjectTypeEnums? ObjectType { get; set; }

    public Genders? Gender { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsPublic { get; set; } = true;

    public string Portrait { get; set; }

    public string Thumbnail { get; set; }

    public VerificationMethods VerificationMethod { get; set; }

    public List<Guid> CategoryIds { get; set; }
}
