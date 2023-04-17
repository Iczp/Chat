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

    public class SessionPermissionRole
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionRole);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
        //public const string DeleteMany = Default + "." + nameof(DeleteMany);
    }

    public class SessionRequest
    {
        public const string Default = GroupName + "." + nameof(SessionRequest);

        public const string Handle = Default + "." + nameof(Handle);
    }
}
