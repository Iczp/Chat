using Volo.Abp.Reflection;

namespace IczpNet.Chat.GlobalPermissions;

public static class GlobalPermissionConsts
{
    public const string GroupName = "Global";
    private static string[] allNames;
    public static string[] GetAll()
    {
        if (allNames == null)
        {
            allNames = ReflectionHelper.GetPublicConstantsRecursively(typeof(GlobalPermissionConsts));
        }
        return allNames;
    }
    public class Room
    {
        public const string Default = GroupName + ".Room";
        public const string UpdateName = Default + "." + nameof(UpdateName);
        public const string UpdatePortrait = Default + "." + nameof(UpdatePortrait);
    }
    public class RoomRole
    {
        public const string Default = GroupName + "." + nameof(RoomRole);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
        //public const string DeleteMany = Default + "." + nameof(DeleteMany);
    }
    public class RoomMember
    {
        public const string Default = GroupName + "." + nameof(RoomMember);
        public const string Create = Default + "." + nameof(Create);
        public const string Delete = Default + "." + nameof(Delete);
        public const string SetRoles = Default + "." + nameof(SetRoles);
    }
}
