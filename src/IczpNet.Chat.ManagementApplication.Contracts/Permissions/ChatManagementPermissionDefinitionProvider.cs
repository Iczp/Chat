using IczpNet.Chat.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace IczpNet.Chat.Management.Permissions;

public class ChatManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var chatGroup = context.AddGroup(ChatManagementPermissions.GroupName, L("Permission:ChatManagement"));

        //SessionPermissionDefinitionPermission
        var definitionPermission = chatGroup.AddPermission(ChatManagementPermissions.SessionPermissionDefinitionPermission.Default, L(ChatManagementPermissions.SessionPermissionDefinitionPermission.Default));
        definitionPermission.AddChild(ChatManagementPermissions.SessionPermissionDefinitionPermission.Update, L(ChatManagementPermissions.SessionPermissionDefinitionPermission.Update));
        definitionPermission.AddChild(ChatManagementPermissions.SessionPermissionDefinitionPermission.SetAllIsEnabled, L(ChatManagementPermissions.SessionPermissionDefinitionPermission.SetAllIsEnabled));
        definitionPermission.AddChild(ChatManagementPermissions.SessionPermissionDefinitionPermission.SetIsEnabled, L(ChatManagementPermissions.SessionPermissionDefinitionPermission.SetIsEnabled));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
