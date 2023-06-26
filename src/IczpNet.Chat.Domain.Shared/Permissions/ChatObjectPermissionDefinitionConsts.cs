using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

public static class ChatObjectPermissionDefinitionConsts
{
    public const string GroupName = "ChatObjectPermission";

    private static string[] allNames;

    public static string[] GetAll()
    {
        allNames ??= ReflectionHelper.GetPublicConstantsRecursively(typeof(ChatObjectPermissionDefinitionConsts));
        return allNames;
    }

    public class MottoPermissions
    {
        private const string Default = GroupName + "." + nameof(MottoPermissions);
        public const string Create = Default + "." + nameof(Create);
    }

}
