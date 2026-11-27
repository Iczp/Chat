using Volo.Abp.Reflection;

namespace IczpNet.Chat.Admin.Permissions;

public static class ChatAdminPermissions
{
    public const string GroupName = "ChatAdmin";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(ChatAdminPermissions));
    }

    public static class ChatObjects
    {
        public const string Default = GroupName + ".ChatObjects";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string SetVerificationMethod = Default + ".SetVerificationMethod";
        public const string SetIsEnabled = Default + ".SetIsEnabled";
    }

    public static class ChatObjectCategories
    {
        public const string Default = GroupName + ".ChatObjectCategories";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class ChatObjectTypes
    {
        public const string Default = GroupName + ".ChatObjectTypes";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class EntryNames
    {
        public const string Default = GroupName + ".EntryNames";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class EntryValues
    {
        public const string Default = GroupName + ".EntryValues";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class InvitationCodes
    {
        public const string Default = GroupName + ".InvitationCodes";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string SetIsEnabled = Default + ".SetIsEnabled";
    }

    public static class UrlSecurityRules
    {
        public const string Default = GroupName + ".UrlSecurityRules";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string SetEnabled = Default + ".SetEnabled";
        public const string RefreshCache = Default + ".RefreshCache";
        public const string CheckUrl = Default + ".CheckUrl";
    }

    public static class SecurityReports
    {
        public const string Default = GroupName + ".SecurityReports";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Handle = Default + ".Handle";
    }

    public static class AiRuns
    {
        public const string Default = GroupName + ".AiRuns";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Retry = Default + ".Retry";
        public const string Cancel = Default + ".Cancel";
        public const string Delete = Default + ".Delete";
    }

    public static class Blobs
    {
        public const string Default = GroupName + ".Blobs";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public static class ScanCodes
    {
        public const string Default = GroupName + ".ScanCodes";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Simulate = Default + ".Simulate";
    }

    public static class MessageReports
    {
        public const string Default = GroupName + ".MessageReports";
        public const string GetList = Default + ".GetList";
        public const string GetSummary = Default + ".GetSummary";
        public const string GetOptions = Default + ".GetOptions";
        public const string Flush = Default + ".Flush";
    }

    public static class Online
    {
        public const string Default = GroupName + ".Online";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Abort = Default + ".Abort";
        public const string Delete = Default + ".Delete";
        public const string ClearAll = Default + ".ClearAll";
    }

    public static class MessageTemplates
    {
        public const string Default = GroupName + ".MessageTemplates";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Publish = Default + ".Publish";
        public const string Share = Default + ".Share";
        public const string Use = Default + ".Use";
        public const string ManageAll = Default + ".ManageAll";
    }

    public static class ArticleMessages
    {
        public const string Default = GroupName + ".ArticleMessages";
        public const string Manage = Default + ".Manage";
        public const string Schedule = Default + ".Schedule";
        public const string Send = Default + ".Send";
        public const string Rollback = Default + ".Rollback";
    }

    public static class SessionUnits
    {
        public const string Default = GroupName + ".SessionUnits";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
    }

    public static class Messages
    {
        public const string Default = GroupName + ".Messages";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string GetContent = Default + ".GetContent";
        public const string Send = Default + ".Send";
        public const string Rollback = Default + ".Rollback";
    }

    public static class Overview
    {
        public const string Default = GroupName + ".Overview";
    }

    public static class Sessions
    {
        public const string Default = GroupName + ".Sessions";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string SetIsEnabled = Default + ".SetIsEnabled";
    }

    public static class SessionRequests
    {
        public const string Default = GroupName + ".SessionRequests";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Handle = Default + ".Handle";
    }

    public static class SessionRoles
    {
        public const string Default = GroupName + ".SessionRoles";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
    }

    public static class SessionAnnouncements
    {
        public const string Default = GroupName + ".SessionAnnouncements";
        public const string GetList = Default + ".GetList";
        public const string GetDetail = Default + ".GetDetail";
        public const string Update = Default + ".Update";
    }
}
