using IczpNet.Chat.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace IczpNet.Chat.Admin.Permissions;

public class ChatAdminPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var adminGroup = context.AddGroup(ChatAdminPermissions.GroupName, L("Permission:ChatAdmin"));

        var chatObjectsPermission = adminGroup.AddPermission(ChatAdminPermissions.ChatObjects.Default, L("Permission:ChatAdmin.ChatObjects"));
        chatObjectsPermission.AddChild(ChatAdminPermissions.ChatObjects.GetList, L("Permission:ChatAdmin.ChatObjects.GetList"));
        chatObjectsPermission.AddChild(ChatAdminPermissions.ChatObjects.GetDetail, L("Permission:ChatAdmin.ChatObjects.GetDetail"));
        chatObjectsPermission.AddChild(ChatAdminPermissions.ChatObjects.Create, L("Permission:ChatAdmin.ChatObjects.Create"));
        chatObjectsPermission.AddChild(ChatAdminPermissions.ChatObjects.Update, L("Permission:ChatAdmin.ChatObjects.Update"));
        chatObjectsPermission.AddChild(ChatAdminPermissions.ChatObjects.Delete, L("Permission:ChatAdmin.ChatObjects.Delete"));
        chatObjectsPermission.AddChild(ChatAdminPermissions.ChatObjects.SetVerificationMethod, L("Permission:ChatAdmin.ChatObjects.SetVerificationMethod"));
        chatObjectsPermission.AddChild(ChatAdminPermissions.ChatObjects.SetIsEnabled, L("Permission:ChatAdmin.ChatObjects.SetIsEnabled"));

        var categoriesPermission = adminGroup.AddPermission(ChatAdminPermissions.ChatObjectCategories.Default, L("Permission:ChatAdmin.ChatObjectCategories"));
        categoriesPermission.AddChild(ChatAdminPermissions.ChatObjectCategories.GetList, L("Permission:ChatAdmin.ChatObjectCategories.GetList"));
        categoriesPermission.AddChild(ChatAdminPermissions.ChatObjectCategories.GetDetail, L("Permission:ChatAdmin.ChatObjectCategories.GetDetail"));
        categoriesPermission.AddChild(ChatAdminPermissions.ChatObjectCategories.Create, L("Permission:ChatAdmin.ChatObjectCategories.Create"));
        categoriesPermission.AddChild(ChatAdminPermissions.ChatObjectCategories.Update, L("Permission:ChatAdmin.ChatObjectCategories.Update"));
        categoriesPermission.AddChild(ChatAdminPermissions.ChatObjectCategories.Delete, L("Permission:ChatAdmin.ChatObjectCategories.Delete"));

        var typesPermission = adminGroup.AddPermission(ChatAdminPermissions.ChatObjectTypes.Default, L("Permission:ChatAdmin.ChatObjectTypes"));
        typesPermission.AddChild(ChatAdminPermissions.ChatObjectTypes.GetList, L("Permission:ChatAdmin.ChatObjectTypes.GetList"));
        typesPermission.AddChild(ChatAdminPermissions.ChatObjectTypes.GetDetail, L("Permission:ChatAdmin.ChatObjectTypes.GetDetail"));
        typesPermission.AddChild(ChatAdminPermissions.ChatObjectTypes.Create, L("Permission:ChatAdmin.ChatObjectTypes.Create"));
        typesPermission.AddChild(ChatAdminPermissions.ChatObjectTypes.Update, L("Permission:ChatAdmin.ChatObjectTypes.Update"));
        typesPermission.AddChild(ChatAdminPermissions.ChatObjectTypes.Delete, L("Permission:ChatAdmin.ChatObjectTypes.Delete"));

        var entryNamesPermission = adminGroup.AddPermission(ChatAdminPermissions.EntryNames.Default, L("Permission:ChatAdmin.EntryNames"));
        entryNamesPermission.AddChild(ChatAdminPermissions.EntryNames.GetList, L("Permission:ChatAdmin.EntryNames.GetList"));
        entryNamesPermission.AddChild(ChatAdminPermissions.EntryNames.GetDetail, L("Permission:ChatAdmin.EntryNames.GetDetail"));
        entryNamesPermission.AddChild(ChatAdminPermissions.EntryNames.Create, L("Permission:ChatAdmin.EntryNames.Create"));
        entryNamesPermission.AddChild(ChatAdminPermissions.EntryNames.Update, L("Permission:ChatAdmin.EntryNames.Update"));
        entryNamesPermission.AddChild(ChatAdminPermissions.EntryNames.Delete, L("Permission:ChatAdmin.EntryNames.Delete"));

        var entryValuesPermission = adminGroup.AddPermission(ChatAdminPermissions.EntryValues.Default, L("Permission:ChatAdmin.EntryValues"));
        entryValuesPermission.AddChild(ChatAdminPermissions.EntryValues.GetList, L("Permission:ChatAdmin.EntryValues.GetList"));
        entryValuesPermission.AddChild(ChatAdminPermissions.EntryValues.GetDetail, L("Permission:ChatAdmin.EntryValues.GetDetail"));
        entryValuesPermission.AddChild(ChatAdminPermissions.EntryValues.Create, L("Permission:ChatAdmin.EntryValues.Create"));
        entryValuesPermission.AddChild(ChatAdminPermissions.EntryValues.Update, L("Permission:ChatAdmin.EntryValues.Update"));
        entryValuesPermission.AddChild(ChatAdminPermissions.EntryValues.Delete, L("Permission:ChatAdmin.EntryValues.Delete"));

        var invitationCodesPermission = adminGroup.AddPermission(ChatAdminPermissions.InvitationCodes.Default, L("Permission:ChatAdmin.InvitationCodes"));
        invitationCodesPermission.AddChild(ChatAdminPermissions.InvitationCodes.GetList, L("Permission:ChatAdmin.InvitationCodes.GetList"));
        invitationCodesPermission.AddChild(ChatAdminPermissions.InvitationCodes.GetDetail, L("Permission:ChatAdmin.InvitationCodes.GetDetail"));
        invitationCodesPermission.AddChild(ChatAdminPermissions.InvitationCodes.Create, L("Permission:ChatAdmin.InvitationCodes.Create"));
        invitationCodesPermission.AddChild(ChatAdminPermissions.InvitationCodes.Update, L("Permission:ChatAdmin.InvitationCodes.Update"));
        invitationCodesPermission.AddChild(ChatAdminPermissions.InvitationCodes.Delete, L("Permission:ChatAdmin.InvitationCodes.Delete"));
        invitationCodesPermission.AddChild(ChatAdminPermissions.InvitationCodes.SetIsEnabled, L("Permission:ChatAdmin.InvitationCodes.SetIsEnabled"));

        var urlRulesPermission = adminGroup.AddPermission(ChatAdminPermissions.UrlSecurityRules.Default, L("Permission:ChatAdmin.UrlSecurityRules"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.GetList, L("Permission:ChatAdmin.UrlSecurityRules.GetList"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.GetDetail, L("Permission:ChatAdmin.UrlSecurityRules.GetDetail"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.Create, L("Permission:ChatAdmin.UrlSecurityRules.Create"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.Update, L("Permission:ChatAdmin.UrlSecurityRules.Update"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.Delete, L("Permission:ChatAdmin.UrlSecurityRules.Delete"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.SetEnabled, L("Permission:ChatAdmin.UrlSecurityRules.SetEnabled"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.RefreshCache, L("Permission:ChatAdmin.UrlSecurityRules.RefreshCache"));
        urlRulesPermission.AddChild(ChatAdminPermissions.UrlSecurityRules.CheckUrl, L("Permission:ChatAdmin.UrlSecurityRules.CheckUrl"));

        var reportsPermission = adminGroup.AddPermission(ChatAdminPermissions.SecurityReports.Default, L("Permission:ChatAdmin.SecurityReports"));
        reportsPermission.AddChild(ChatAdminPermissions.SecurityReports.GetList, L("Permission:ChatAdmin.SecurityReports.GetList"));
        reportsPermission.AddChild(ChatAdminPermissions.SecurityReports.GetDetail, L("Permission:ChatAdmin.SecurityReports.GetDetail"));
        reportsPermission.AddChild(ChatAdminPermissions.SecurityReports.Handle, L("Permission:ChatAdmin.SecurityReports.Handle"));

        var aiRunsPermission = adminGroup.AddPermission(ChatAdminPermissions.AiRuns.Default, L("Permission:ChatAdmin.AiRuns"));
        aiRunsPermission.AddChild(ChatAdminPermissions.AiRuns.GetList, L("Permission:ChatAdmin.AiRuns.GetList"));
        aiRunsPermission.AddChild(ChatAdminPermissions.AiRuns.GetDetail, L("Permission:ChatAdmin.AiRuns.GetDetail"));
        aiRunsPermission.AddChild(ChatAdminPermissions.AiRuns.Retry, L("Permission:ChatAdmin.AiRuns.Retry"));
        aiRunsPermission.AddChild(ChatAdminPermissions.AiRuns.Cancel, L("Permission:ChatAdmin.AiRuns.Cancel"));
        aiRunsPermission.AddChild(ChatAdminPermissions.AiRuns.Delete, L("Permission:ChatAdmin.AiRuns.Delete"));

        var blobsPermission = adminGroup.AddPermission(ChatAdminPermissions.Blobs.Default, L("Permission:ChatAdmin.Blobs"));
        blobsPermission.AddChild(ChatAdminPermissions.Blobs.GetList, L("Permission:ChatAdmin.Blobs.GetList"));
        blobsPermission.AddChild(ChatAdminPermissions.Blobs.GetDetail, L("Permission:ChatAdmin.Blobs.GetDetail"));
        blobsPermission.AddChild(ChatAdminPermissions.Blobs.Update, L("Permission:ChatAdmin.Blobs.Update"));
        blobsPermission.AddChild(ChatAdminPermissions.Blobs.Delete, L("Permission:ChatAdmin.Blobs.Delete"));

        var scanCodesPermission = adminGroup.AddPermission(ChatAdminPermissions.ScanCodes.Default, L("Permission:ChatAdmin.ScanCodes"));
        scanCodesPermission.AddChild(ChatAdminPermissions.ScanCodes.GetList, L("Permission:ChatAdmin.ScanCodes.GetList"));
        scanCodesPermission.AddChild(ChatAdminPermissions.ScanCodes.GetDetail, L("Permission:ChatAdmin.ScanCodes.GetDetail"));
        scanCodesPermission.AddChild(ChatAdminPermissions.ScanCodes.Simulate, L("Permission:ChatAdmin.ScanCodes.Simulate"));

        var messageReportsPermission = adminGroup.AddPermission(ChatAdminPermissions.MessageReports.Default, L("Permission:ChatAdmin.MessageReports"));
        messageReportsPermission.AddChild(ChatAdminPermissions.MessageReports.GetList, L("Permission:ChatAdmin.MessageReports.GetList"));
        messageReportsPermission.AddChild(ChatAdminPermissions.MessageReports.GetSummary, L("Permission:ChatAdmin.MessageReports.GetSummary"));
        messageReportsPermission.AddChild(ChatAdminPermissions.MessageReports.GetOptions, L("Permission:ChatAdmin.MessageReports.GetOptions"));
        messageReportsPermission.AddChild(ChatAdminPermissions.MessageReports.Flush, L("Permission:ChatAdmin.MessageReports.Flush"));

        var onlinePermission = adminGroup.AddPermission(ChatAdminPermissions.Online.Default, L("Permission:ChatAdmin.Online"));
        onlinePermission.AddChild(ChatAdminPermissions.Online.GetList, L("Permission:ChatAdmin.Online.GetList"));
        onlinePermission.AddChild(ChatAdminPermissions.Online.GetDetail, L("Permission:ChatAdmin.Online.GetDetail"));
        onlinePermission.AddChild(ChatAdminPermissions.Online.Abort, L("Permission:ChatAdmin.Online.Abort"));
        onlinePermission.AddChild(ChatAdminPermissions.Online.Delete, L("Permission:ChatAdmin.Online.Delete"));
        onlinePermission.AddChild(ChatAdminPermissions.Online.ClearAll, L("Permission:ChatAdmin.Online.ClearAll"));

        var templatesPermission = adminGroup.AddPermission(ChatAdminPermissions.MessageTemplates.Default, L("Permission:ChatAdmin.MessageTemplates"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.GetList, L("Permission:ChatAdmin.MessageTemplates.GetList"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.GetDetail, L("Permission:ChatAdmin.MessageTemplates.GetDetail"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.Create, L("Permission:ChatAdmin.MessageTemplates.Create"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.Update, L("Permission:ChatAdmin.MessageTemplates.Update"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.Publish, L("Permission:ChatAdmin.MessageTemplates.Publish"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.Share, L("Permission:ChatAdmin.MessageTemplates.Share"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.Use, L("Permission:ChatAdmin.MessageTemplates.Use"));
        templatesPermission.AddChild(ChatAdminPermissions.MessageTemplates.ManageAll, L("Permission:ChatAdmin.MessageTemplates.ManageAll"));

        var articlePermission = adminGroup.AddPermission(ChatAdminPermissions.ArticleMessages.Default, L("Permission:ChatAdmin.ArticleMessages"));
        articlePermission.AddChild(ChatAdminPermissions.ArticleMessages.Manage, L("Permission:ChatAdmin.ArticleMessages.Manage"));
        articlePermission.AddChild(ChatAdminPermissions.ArticleMessages.Schedule, L("Permission:ChatAdmin.ArticleMessages.Schedule"));
        articlePermission.AddChild(ChatAdminPermissions.ArticleMessages.Send, L("Permission:ChatAdmin.ArticleMessages.Send"));
        articlePermission.AddChild(ChatAdminPermissions.ArticleMessages.Rollback, L("Permission:ChatAdmin.ArticleMessages.Rollback"));

        var sessionUnitsPermission = adminGroup.AddPermission(ChatAdminPermissions.SessionUnits.Default, L("Permission:ChatAdmin.SessionUnits"));
        sessionUnitsPermission.AddChild(ChatAdminPermissions.SessionUnits.GetList, L("Permission:ChatAdmin.SessionUnits.GetList"));
        sessionUnitsPermission.AddChild(ChatAdminPermissions.SessionUnits.GetDetail, L("Permission:ChatAdmin.SessionUnits.GetDetail"));

        var messagesPermission = adminGroup.AddPermission(ChatAdminPermissions.Messages.Default, L("Permission:ChatAdmin.Messages"));
        messagesPermission.AddChild(ChatAdminPermissions.Messages.GetList, L("Permission:ChatAdmin.Messages.GetList"));
        messagesPermission.AddChild(ChatAdminPermissions.Messages.GetDetail, L("Permission:ChatAdmin.Messages.GetDetail"));
        messagesPermission.AddChild(ChatAdminPermissions.Messages.GetContent, L("Permission:ChatAdmin.Messages.GetContent"));
        messagesPermission.AddChild(ChatAdminPermissions.Messages.Send, L("Permission:ChatAdmin.Messages.Send"));
        messagesPermission.AddChild(ChatAdminPermissions.Messages.Rollback, L("Permission:ChatAdmin.Messages.Rollback"));

        adminGroup.AddPermission(ChatAdminPermissions.Overview.Default, L("Permission:ChatAdmin.Overview"));

        var sessionsPermission = adminGroup.AddPermission(ChatAdminPermissions.Sessions.Default, L("Permission:ChatAdmin.Sessions"));
        sessionsPermission.AddChild(ChatAdminPermissions.Sessions.GetList, L("Permission:ChatAdmin.Sessions.GetList"));
        sessionsPermission.AddChild(ChatAdminPermissions.Sessions.GetDetail, L("Permission:ChatAdmin.Sessions.GetDetail"));
        sessionsPermission.AddChild(ChatAdminPermissions.Sessions.SetIsEnabled, L("Permission:ChatAdmin.Sessions.SetIsEnabled"));

        var sessionRequestsPermission = adminGroup.AddPermission(ChatAdminPermissions.SessionRequests.Default, L("Permission:ChatAdmin.SessionRequests"));
        sessionRequestsPermission.AddChild(ChatAdminPermissions.SessionRequests.GetList, L("Permission:ChatAdmin.SessionRequests.GetList"));
        sessionRequestsPermission.AddChild(ChatAdminPermissions.SessionRequests.GetDetail, L("Permission:ChatAdmin.SessionRequests.GetDetail"));
        sessionRequestsPermission.AddChild(ChatAdminPermissions.SessionRequests.Handle, L("Permission:ChatAdmin.SessionRequests.Handle"));

        var sessionRolesPermission = adminGroup.AddPermission(ChatAdminPermissions.SessionRoles.Default, L("Permission:ChatAdmin.SessionRoles"));
        sessionRolesPermission.AddChild(ChatAdminPermissions.SessionRoles.GetList, L("Permission:ChatAdmin.SessionRoles.GetList"));
        sessionRolesPermission.AddChild(ChatAdminPermissions.SessionRoles.GetDetail, L("Permission:ChatAdmin.SessionRoles.GetDetail"));

        var sessionAnnouncementsPermission = adminGroup.AddPermission(ChatAdminPermissions.SessionAnnouncements.Default, L("Permission:ChatAdmin.SessionAnnouncements"));
        sessionAnnouncementsPermission.AddChild(ChatAdminPermissions.SessionAnnouncements.GetList, L("Permission:ChatAdmin.SessionAnnouncements.GetList"));
        sessionAnnouncementsPermission.AddChild(ChatAdminPermissions.SessionAnnouncements.GetDetail, L("Permission:ChatAdmin.SessionAnnouncements.GetDetail"));
        sessionAnnouncementsPermission.AddChild(ChatAdminPermissions.SessionAnnouncements.Update, L("Permission:ChatAdmin.SessionAnnouncements.Update"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatResource>(name);
    }
}
