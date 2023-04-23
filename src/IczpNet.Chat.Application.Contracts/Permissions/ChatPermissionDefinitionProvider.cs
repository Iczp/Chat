using IczpNet.Chat.Localization;
using IczpNet.Chat.SessionSections.SessionPermissionDefinitions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace IczpNet.Chat.Permissions;

public class ChatPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ChatPermissions.GroupName, L("Permission:Chat"));

        var sessionGroup = context.AddGroup(SessionPermissionDefinitionConsts.GroupName, L($"Permission:{SessionPermissionDefinitionConsts.GroupName}"));

        //SessionPermissionDefinitionPermission
        var definitionPermission = sessionGroup.AddPermission(SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.Default, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default));
        definitionPermission.AddChild(SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.Update, L(SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.Update));
        definitionPermission.AddChild(SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.SetAllIsEnabled, L(SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.SetAllIsEnabled));
        definitionPermission.AddChild(SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.SetIsEnabled, L(SessionPermissionDefinitionConsts.SessionPermissionDefinitionPermission.SetIsEnabled));


        //session organization
        var organizationPermission = sessionGroup.AddPermission(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default));
        organizationPermission.AddChild(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create));
        organizationPermission.AddChild(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update));
        organizationPermission.AddChild(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete));

        //sesson role
        var rolePermission = sessionGroup.AddPermission(SessionPermissionDefinitionConsts.SessionRolePermission.Default, L(SessionPermissionDefinitionConsts.SessionRolePermission.Default));
        rolePermission.AddChild(SessionPermissionDefinitionConsts.SessionRolePermission.Create, L(SessionPermissionDefinitionConsts.SessionRolePermission.Create));
        rolePermission.AddChild(SessionPermissionDefinitionConsts.SessionRolePermission.Update, L(SessionPermissionDefinitionConsts.SessionRolePermission.Update));
        rolePermission.AddChild(SessionPermissionDefinitionConsts.SessionRolePermission.Delete, L(SessionPermissionDefinitionConsts.SessionRolePermission.Delete));
        rolePermission.AddChild(SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions, L(SessionPermissionDefinitionConsts.SessionRolePermission.SetAllPermissions));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
