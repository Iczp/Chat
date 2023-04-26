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
        var definitionPermission = chatGroup.AddPermission(ChatPermissions.SessionPermissionDefinitionPermission.Default, L(ChatPermissions.SessionPermissionDefinitionPermission.Default));
        definitionPermission.AddChild(ChatPermissions.SessionPermissionDefinitionPermission.Update, L(ChatPermissions.SessionPermissionDefinitionPermission.Update));
        definitionPermission.AddChild(ChatPermissions.SessionPermissionDefinitionPermission.SetAllIsEnabled, L(ChatPermissions.SessionPermissionDefinitionPermission.SetAllIsEnabled));
        definitionPermission.AddChild(ChatPermissions.SessionPermissionDefinitionPermission.SetIsEnabled, L(ChatPermissions.SessionPermissionDefinitionPermission.SetIsEnabled));

        //SessionPermissionDefinitionPermission
        var robotManagementPermission = chatGroup.AddPermission(ChatPermissions.RobotManagementPermission.Default, L(ChatPermissions.RobotManagementPermission.Default));
        definitionPermission.AddChild(ChatPermissions.RobotManagementPermission.Create, L(ChatPermissions.RobotManagementPermission.Create));
        definitionPermission.AddChild(ChatPermissions.RobotManagementPermission.Update, L(ChatPermissions.RobotManagementPermission.Update));
        definitionPermission.AddChild(ChatPermissions.RobotManagementPermission.SetIsEnabled, L(ChatPermissions.RobotManagementPermission.SetIsEnabled));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
