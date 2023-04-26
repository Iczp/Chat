using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

public class ChatPermissions
{
    public const string GroupName = "Chat";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ChatPermissions));
    }

    public class SessionPermissionDefinitionPermission
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionDefinitionPermission);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
        public const string SetAllIsEnabled = Default + "." + nameof(SetAllIsEnabled);

    }

    public class RobotManagementPermission
    {
        public const string Default = GroupName + "." + nameof(RobotManagementPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
    }
}
