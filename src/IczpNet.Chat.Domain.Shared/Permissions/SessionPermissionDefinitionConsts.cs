using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

public static class SessionPermissionDefinitionConsts
{
    public const string GroupName = "SessionPermission";

    private static string[] allNames;

    public static string[] GetAll()
    {
        allNames ??= ReflectionHelper.GetPublicConstantsRecursively(typeof(SessionPermissionDefinitionConsts));
        return allNames;
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
        public const string ToggleAllInputEnabled = Default + "." + nameof(ToggleAllInputEnabled);
    }

    public class SessionUnitPermissions
    {
        public const string Default = GroupName + "." + nameof(SessionUnitPermissions);
        public const string RemindEveryone = Default + "." + nameof(RemindEveryone);
        public const string RemoveMember = Default + "." + nameof(RemoveMember);
        public const string RollbackOthersMessage = Default + "." + nameof(RollbackOthersMessage);
        public const string ToggleInputEnabled = Default + "." + nameof(ToggleInputEnabled);
    }
}
