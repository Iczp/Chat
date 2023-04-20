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

        var organizationPermission = sessionGroup.AddPermission(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Default));
        organizationPermission.AddChild(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Create));
        organizationPermission.AddChild(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Update));
        organizationPermission.AddChild(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete, L(SessionPermissionDefinitionConsts.SessionOrganizationPermission.Delete));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
