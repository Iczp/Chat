using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IczpNet.Chat.TextTemplates;

public class SessionTextTemplate : TextTemplate
{
    /// <summary>
    /// 自定义协议 Scheme（默认 "im"，可通过配置或启动时修改为其他协议）
    /// </summary>
    public static string Scheme { get; set; } = "im";

    /// <summary>
    /// 标签模板格式（默认 <![CDATA[<a href="{Url}">{DisplayText}</a>]]>）
    /// </summary>
    public static string LinkTemplate { get; set; } = "<a href=\"{Url}\">{DisplayText}</a>";

    public override string Text { get; protected set; } = LinkTemplate;

    public string Url { get; set; }

    public string DisplayText { get; set; }

    public SessionTextTemplate(string url, string displayText)
    {
        Url = url;
        DisplayText = displayText ?? string.Empty;
        SetData();
    }

    public SessionTextTemplate(string route, IDictionary<string, string> query, string displayText, string scheme = null)
        : this(BuildUri(route, query, scheme), displayText)
    {
    }

    private void SetData()
    {
        Data[nameof(Url)] = WebUtility.HtmlEncode(Url ?? string.Empty);
        Data[nameof(DisplayText)] = EscapeDisplayText(DisplayText);
    }

    public override string ToString()
    {
        SetData();
        return base.ToString();
    }

    public static string EscapeDisplayText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    public static string BuildUri(string route, IDictionary<string, string> query = null, string scheme = null)
    {
        var activeScheme = string.IsNullOrWhiteSpace(scheme) ? Scheme : scheme.Trim();
        var cleanRoute = (route ?? string.Empty).Trim().Trim('/');
        if (query == null || query.Count == 0)
        {
            return $"{activeScheme}://{cleanRoute}";
        }
        var pairs = query
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && kv.Value != null)
            .Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}");
        var qs = string.Join("&", pairs);
        return string.IsNullOrEmpty(qs) ? $"{activeScheme}://{cleanRoute}" : $"{activeScheme}://{cleanRoute}?{qs}";
    }

    public static SessionTextTemplate Route(string route, string id, string displayText, string scheme = null)
    {
        var query = string.IsNullOrWhiteSpace(id)
            ? null
            : new Dictionary<string, string> { ["id"] = id };
        return new SessionTextTemplate(route, query, displayText, scheme);
    }

    public static SessionTextTemplate SessionUnit(Guid sessionUnitId, string name, string scheme = null)
        => Route("session-unit", sessionUnitId.ToString(), name, scheme);

    public static SessionTextTemplate SessionUnit(IEnumerable<Guid> sessionUnitIds, string name, string scheme = null)
        => Route("session-unit", sessionUnitIds?.JoinAsString(",") ?? string.Empty, name, scheme);

    public static SessionTextTemplate SessionUnit(SessionUnit sessionUnit, string scheme = null)
    {
        var name = !sessionUnit.Setting?.MemberName.IsNullOrWhiteSpace() == true
            ? sessionUnit.Setting.MemberName
            : sessionUnit.Owner?.Name ?? string.Empty;
        return SessionUnit(sessionUnit.Id, name, scheme);
    }

    public static SessionTextTemplate SessionUnit(SessionUnitCacheItem sessionUnit, string scheme = null)
    {
        var name = !sessionUnit.MemberName.IsNullOrWhiteSpace()
            ? sessionUnit.MemberName
            : sessionUnit.OwnerName ?? string.Empty;
        return SessionUnit(sessionUnit.Id, name, scheme);
    }

    public static SessionTextTemplate ChatObject(long chatObjectId, string name, string scheme = null)
        => Route("chat-object", chatObjectId.ToString(), name, scheme);

    public static SessionTextTemplate ChatObject(IChatObject chatObject, string scheme = null)
        => ChatObject(chatObject.Id, chatObject.Name, scheme);

    public static SessionTextTemplate Announcement(Guid announcementId, string title, string scheme = null)
        => Route("announcement", announcementId.ToString(), title, scheme);
}
