using IczpNet.Chat.Localization;
using System.Linq;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.Reflection;

namespace IczpNet.Chat.Permissions;

public class ChatPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var chatGroup = context.AddGroup(ChatPermissions.GroupName, L("Permission:Chat"));

        //SessionPermissionDefinitionPermission

        //chatGroup.AddPermission<ChatPermissions.SessionPermissionDefinitionPermission>();

        //chatGroup.AddPermission<ChatPermissions.RobotManagementPermission>();

        //chatGroup.AddPermission<ChatPermissions.RoomManagementPermission>();

        //chatGroup.AddPermission<ChatPermissions.MessageSenderPermission>();

        chatGroup.AddPermissions<ChatPermissions>();

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
