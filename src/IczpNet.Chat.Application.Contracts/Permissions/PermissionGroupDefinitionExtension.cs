using IczpNet.Chat.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions
{
    internal static class PermissionGroupDefinitionExtension
    {
        public static void AddPermission(this PermissionGroupDefinition group, string[] names)
        {
            var permission = group.AddPermission(names[0], L(names[0]));

            for (int i = 1; i < names.Length; i++)
            {
                permission.AddChild(names[i], L(names[i]));
            }
        }

        public static void AddPermission<T>(this PermissionGroupDefinition group)
        {
            var names = ReflectionHelper.GetPublicConstantsRecursively(typeof(T));

            AddPermission(group, names);
        }

        public static LocalizableString L(string name)
        {
            return LocalizableString.Create<ChatResource>(name);
        }
    }
}
