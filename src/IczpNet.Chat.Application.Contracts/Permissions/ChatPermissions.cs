using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

public class ChatPermissions
{
    public const string GroupName = "Chat";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ChatPermissions));
    }

    public class ChatObjectPermission
    {
        public const string Default = GroupName + "." + nameof(ChatObjectPermission);
        public const string GetItem = Default + "." + nameof(GetItem);
        public const string GetList = Default + "." + nameof(GetList);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public class DeveloperPermission
    {
        public const string Default = GroupName + "." + nameof(DeveloperPermission);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
    }

    public class ChatObjectCategoryPermission
    {
        public const string Default = GroupName + "." + nameof(ChatObjectCategoryPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public class ChatObjectTypePermission
    {
        public const string Default = GroupName + "." + nameof(ChatObjectTypePermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }

    

    public class MessageSenderPermission
    {
        public const string Default = GroupName + "." + nameof(MessageSenderPermission);
        
        public const string SendCommand = Default + "." + nameof(SendCommand);
        public const string SendText = Default + "." + nameof(SendText);
        public const string SendLink = Default + "." + nameof(SendLink);
        public const string SendLocation = Default + "." + nameof(SendLocation);
        public const string SendImage = Default + "." + nameof(SendImage);
        public const string SendSound = Default + "." + nameof(SendSound);
        public const string SendVideo = Default + "." + nameof(SendVideo);
        public const string SendFile = Default + "." + nameof(SendFile);
        public const string SendRedpackage = Default + "." + nameof(SendRedpackage);
        public const string SendHistory = Default + "." + nameof(SendHistory);

        public const string Rollback = Default + "." + nameof(Rollback);
        public const string Forward = Default + "." + nameof(Forward);
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

    public class RoomManagementPermission
    {
        public const string Default = GroupName + "." + nameof(RoomManagementPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
    }

    public class EntryNamePermission
    {
        public const string Default = GroupName + "." + nameof(EntryNamePermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }
    public class EntryValuePermission
    {
        public const string Default = GroupName + "." + nameof(EntryValuePermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }
}
