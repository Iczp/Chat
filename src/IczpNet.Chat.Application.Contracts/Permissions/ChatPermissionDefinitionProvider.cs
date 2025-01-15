using IczpNet.Chat.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

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

        chatGroup.AddPermissions<ChatPermissions>(x => L($"{x}"));

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
