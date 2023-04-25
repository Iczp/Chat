using Volo.Abp.Reflection;

namespace IczpNet.Chat.Management.Permissions;

public class ChatManagementPermissions
{
    public const string GroupName = "ChatManagement";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ChatManagementPermissions));
    }

    public class SessionPermissionDefinitionPermission
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionDefinitionPermission);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
        public const string SetAllIsEnabled = Default + "." + nameof(SetAllIsEnabled);

    }
}
