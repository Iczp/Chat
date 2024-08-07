﻿using System.ComponentModel;
using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

/// <summary>
/// 会话内权限
/// </summary>
public class SessionPermissionDefinitionConsts
{
    public const string GroupName = nameof(SessionPermissionDefinitionConsts);

    private static string[] allNames;

    public static string[] GetAll()
    {
        allNames ??= ReflectionHelper.GetPublicConstantsRecursively(typeof(SessionPermissionDefinitionConsts));
        return allNames;
    }

    public class SessionTagPermission
    {
        public const string Default = GroupName + "." + nameof(SessionTagPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public class SessionRolePermission
    {
        public const string Default = GroupName + "." + nameof(SessionRolePermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
        public const string SetAllPermissions = Default + "." + nameof(SetAllPermissions);
    }

    public class SessionRequestPermission
    {
        public const string Default = GroupName + "." + nameof(SessionRequestPermission);
        public const string Handle = Default + "." + nameof(Handle);
    }

    public class SessionOrganizationPermission
    {
        public const string Default = GroupName + "." + nameof(SessionOrganizationPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public class ChatObjectPermission
    {
        public const string Default = GroupName + "." + nameof(ChatObjectPermission);
        public const string UpdateName = Default + "." + nameof(UpdateName);
        public const string UpdatePortrait = Default + "." + nameof(UpdatePortrait);
        public const string ToggleAllInputEnabled = Default + "." + nameof(ToggleAllInputEnabled);
        public const string VerificationMethod = Default + "." + nameof(VerificationMethod);
        public const string SetServiceStatus = Default + "." + nameof(SetServiceStatus);

    }

    public class SessionUnitPermissions
    {
        public const string Default = GroupName + "." + nameof(SessionUnitPermissions);
        public const string RemindEveryone = Default + "." + nameof(RemindEveryone);
        public const string RemoveMember = Default + "." + nameof(RemoveMember);
        public const string RollbackOthersMessage = Default + "." + nameof(RollbackOthersMessage);
        public const string ToggleInputEnabled = Default + "." + nameof(ToggleInputEnabled);
        [Description("设置禁言过期时间")]
        public const string SetMuteExpireTime = Default + "." + nameof(SetMuteExpireTime);
    }
}
