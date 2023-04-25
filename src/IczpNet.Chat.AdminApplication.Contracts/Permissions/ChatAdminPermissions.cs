using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

public class ChatAdminPermissions
{
    public const string GroupName = "ChatAdmin";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ChatAdminPermissions));
    }

    public class SessionPermissionDefinitionPermission
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionDefinitionPermission);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
        public const string SetAllIsEnabled = Default + "." + nameof(SetAllIsEnabled);

    }
}
