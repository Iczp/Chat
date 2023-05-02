﻿using IczpNet.Chat.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace IczpNet.Chat.Permissions;

public class SessionPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {

        var sessionGroup = context.AddGroup(SessionPermissionDefinitionConsts.GroupName, L($"Permission:{SessionPermissionDefinitionConsts.GroupName}"));

        sessionGroup.AddPermission<SessionPermissionDefinitionConsts.SessionOrganizationPermission>();

        sessionGroup.AddPermission<SessionPermissionDefinitionConsts.SessionRolePermission>();
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}