using Volo.Abp.Reflection;

namespace IczpNet.Chat.SessionSections.SessionPermissionDefinitions;

public static class SessionPermissionDefinitionConsts
{
    public const string GroupName = "SessionPermission";

    private static string[] allNames;

    public static string[] GetAll()
    {
        allNames ??= ReflectionHelper.GetPublicConstantsRecursively(typeof(SessionPermissionDefinitionConsts));
        return allNames;
    }

    public class SessionPermissionDefinitionPermission
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionDefinitionPermission);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
        public const string SetAllIsEnabled = Default + "." + nameof(SetAllIsEnabled);
        
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

    public class SessionPermissionGroupPermission
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionGroupPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public class ChatObjectPermission
    {
        public const string Default = GroupName + "." + nameof(ChatObjectPermission);
        public const string UpdateName = Default + "." + nameof(UpdateName);
        public const string UpdatePortrait = Default + "." + nameof(UpdatePortrait);
    }
}
