using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

public class ChatPermissions
{
    public const string GroupName = nameof(ChatPermissions);

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

    public class ContactPermission
    {
        public const string Default = GroupName + "." + nameof(ContactPermission);
        public const string GetList = Default + "." + nameof(GetList);
        public const string GetItem = Default + "." + nameof(GetItem);
        public const string Delete = Default + "." + nameof(Delete);
    }
    public class ContactTagPermission
    {
        public const string Default = GroupName + "." + nameof(ContactTagPermission);
        public const string GetList = Default + "." + nameof(GetList);
        public const string GetItem = Default + "." + nameof(GetItem);
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


    public class SessionPermissionGroupPermission
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionGroupPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public class SessionPermissionDefinitionPermission
    {
        public const string Default = GroupName + "." + nameof(SessionPermissionDefinitionPermission);
        public const string Update = Default + "." + nameof(Update);
        public const string SetIsEnabled = Default + "." + nameof(SetIsEnabled);
        public const string SetAllIsEnabled = Default + "." + nameof(SetAllIsEnabled);

    }

    public class SessionUnitPermissions
    {
        public const string Default = GroupName + "." + nameof(SessionUnitPermissions);
        public const string MessageBus = Default + "." + nameof(MessageBus);
        public const string GetSameSession = Default + "." + nameof(GetSameSession);
        public const string GetBadge  = Default + "." + nameof(GetBadge);
        public const string Find = Default + "." + nameof(Find);
        public const string GetCounter = Default + "." + nameof(GetCounter);
    }

    public class SessionUnitSettingPermissions
    {
        public const string Default = GroupName + "." + nameof(SessionUnitSettingPermissions);
        public const string SetRename = Default + "." + nameof(SetRename);
        public const string SetMemberName = Default + "." + nameof(SetMemberName);
        public const string SetReaded = Default + "." + nameof(SetReaded);
        public const string SetTopping = Default + "." + nameof(SetTopping);
        public const string SetImmersed = Default + "." + nameof(SetImmersed);
        public const string SetIsContacts = Default + "." + nameof(SetIsContacts);
        public const string SetIsShowMemberName = Default + "." + nameof(SetIsShowMemberName);
        public const string RemoveSession = Default + "." + nameof(RemoveSession);
        public const string ClearMessage = Default + "." + nameof(ClearMessage);
        public const string DeleteMessage = Default + "." + nameof(DeleteMessage);
        public const string SetContactTags = Default + "." + nameof(SetContactTags);
        public const string Kill = Default + "." + nameof(Kill);
        public const string SetMuteExpireTime = Default + "." + nameof(SetMuteExpireTime);
    }

    public class SessionRequestPermissions
    {
        public const string Default = GroupName + "." + nameof(SessionRequestPermissions);
        public const string GetAll = Default + "." + nameof(GetAll);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
        public const string HandleRquest = Default + "." + nameof(HandleRquest);
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

    public class WordPermission
    {
        public const string Default = GroupName + "." + nameof(WordPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
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

    public class WalletOrderPermission
    {
        public const string Default = GroupName + "." + nameof(WalletOrderPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }
    public class BlobPermission
    {
        public const string Default = GroupName + "." + nameof(BlobPermission);
        public const string Create = Default + "." + nameof(Create);
        public const string Update = Default + "." + nameof(Update);
        public const string Delete = Default + "." + nameof(Delete);
    }
}
