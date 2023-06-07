using IczpNet.Chat.Localization;
using System;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions
{
    public static class PermissionGroupDefinitionExtension
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
            AddPermission(group, typeof(T));
        }

        public static void AddPermission(this PermissionGroupDefinition group, Type type)
        {
            var names = ReflectionHelper.GetPublicConstantsRecursively(type);

            AddPermission(group, names);
        }

        public static void AddPermissions(this PermissionGroupDefinition group, Type rootPermissionType)
        {
            foreach (var nestedType in rootPermissionType.GetNestedTypes())
            {
                AddPermission(group, nestedType);
            }
        }
        public static void AddPermissions<T>(this PermissionGroupDefinition group)
        {
            AddPermissions(group, typeof(T));
        }
        public static LocalizableString L(string name)
        {
            return LocalizableString.Create<ChatResource>(name);
        }
    }
}
