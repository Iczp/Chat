using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IczpNet.Chat.QrCodes;

public enum QrCodeParsedKind
{
    None = 0,
    CodeTicket = 1,
    LegacyChatObjectId = 2,
}

public sealed class QrCodeParsedTarget
{
    public QrCodeParsedKind Kind { get; init; }

    public string Code { get; init; }

    public long? LegacyChatObjectId { get; init; }

    public bool IsGroupHint { get; init; }
}

/// <summary>
/// 二维码内容模板格式化与识别解析工具
/// </summary>
public static class QrCodeTemplateMatcher
{
    public const string CodePlaceholder = "{code}";

    public static string FormatContent(string template, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or whitespace.", nameof(code));
        }

        var effectiveTemplate = string.IsNullOrWhiteSpace(template) || !template.Contains(CodePlaceholder, StringComparison.OrdinalIgnoreCase)
            ? "gotoim://qr/{code}"
            : template;

        return Regex.Replace(
            effectiveTemplate,
            Regex.Escape(CodePlaceholder),
            code,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    public static bool TryMatchTemplate(string template, string input, out IReadOnlyDictionary<string, string> values)
    {
        values = null;
        if (string.IsNullOrWhiteSpace(template) || string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var escaped = Regex.Escape(template.Trim())
            .Replace(@"\{", "{")
            .Replace(@"\}", "}");

        var pattern = "^" + Regex.Replace(escaped, @"\{([^}]+)\}", m =>
        {
            var key = m.Groups[1].Value;
            return $"(?<{key}>[^/?&#\\s]+)";
        }) + "$";

        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        var match = regex.Match(input.Trim());
        if (!match.Success)
        {
            return false;
        }

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var groupName in regex.GetGroupNames())
        {
            if (groupName != "0")
            {
                dict[groupName] = Uri.UnescapeDataString(match.Groups[groupName].Value);
            }
        }

        values = dict;
        return true;
    }

    public static bool TryExtractCodeFromTemplates(IEnumerable<string> templates, string input, out string code)
    {
        code = null;
        if (string.IsNullOrWhiteSpace(input) || templates == null)
        {
            return false;
        }

        foreach (var template in templates.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (TryMatchTemplate(template, input, out var values) &&
                values.TryGetValue("code", out var matched) &&
                !string.IsNullOrWhiteSpace(matched))
            {
                code = matched.Trim();
                return true;
            }
        }

        return false;
    }

    public static QrCodeParsedTarget ParseQrContent(string rawContent, QrCodeOptions options = null)
    {
        if (string.IsNullOrWhiteSpace(rawContent))
        {
            return null;
        }

        var text = rawContent.Trim();
        var effectiveOptions = options ?? new QrCodeOptions();

        // 1. 优先匹配配置的 ContentTemplate 与 CompatibleTemplates
        var candidateTemplates = new List<string> { effectiveOptions.ContentTemplate };
        if (effectiveOptions.CompatibleTemplates != null)
        {
            candidateTemplates.AddRange(effectiveOptions.CompatibleTemplates);
        }

        if (TryExtractCodeFromTemplates(candidateTemplates, text, out var templateCode))
        {
            return new QrCodeParsedTarget
            {
                Kind = QrCodeParsedKind.CodeTicket,
                Code = templateCode
            };
        }

        // 2. URI 结构化解析（兼容 Universal Links https://*/q/{code}、gotoim://qr/{code}、旧版 gotoim://user 等）
        if (Uri.TryCreate(text, UriKind.Absolute, out var uri))
        {
            var scheme = uri.Scheme.ToLowerInvariant();
            var isCustomScheme = scheme is "gotoim" or "gotoim-dev";
            var isHttpScheme = scheme is "http" or "https";

            if (isCustomScheme || isHttpScheme)
            {
                var segments = new List<string>();
                if (isCustomScheme && !string.IsNullOrWhiteSpace(uri.Host))
                {
                    segments.Add(Uri.UnescapeDataString(uri.Host));
                }

                foreach (var seg in uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    segments.Add(Uri.UnescapeDataString(seg));
                }

                var queryDict = ParseQueryString(uri.Query);

                if (segments.Count > 0)
                {
                    var first = segments[0].ToLowerInvariant();

                    // gotoim://qr/{code} 或 https://domain/q/{code} 或 gotoim://card/{code}
                    if (first is "qr" or "q" or "card")
                    {
                        var segCode = segments.Count >= 2 ? segments[1] : null;
                        if (string.IsNullOrWhiteSpace(segCode))
                        {
                            queryDict.TryGetValue("code", out segCode);
                            if (string.IsNullOrWhiteSpace(segCode))
                            {
                                queryDict.TryGetValue("ticket", out segCode);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(segCode))
                        {
                            return new QrCodeParsedTarget
                            {
                                Kind = QrCodeParsedKind.CodeTicket,
                                Code = segCode.Trim()
                            };
                        }
                    }

                    // gotoim://invite/group/{code}
                    if (first == "invite" && segments.Count >= 3 && segments[1].Equals("group", StringComparison.OrdinalIgnoreCase))
                    {
                        return new QrCodeParsedTarget
                        {
                            Kind = QrCodeParsedKind.CodeTicket,
                            Code = segments[2].Trim(),
                            IsGroupHint = true
                        };
                    }

                    // 兼容 gotoim://user/{id} 或 gotoim://user/?id={id} 或 gotoim://user?ticket={code}
                    if (first == "user")
                    {
                        if (queryDict.TryGetValue("ticket", out var userTicket) && !string.IsNullOrWhiteSpace(userTicket))
                        {
                            return new QrCodeParsedTarget
                            {
                                Kind = QrCodeParsedKind.CodeTicket,
                                Code = userTicket.Trim()
                            };
                        }

                        if (queryDict.TryGetValue("code", out var userCode) && !string.IsNullOrWhiteSpace(userCode))
                        {
                            return new QrCodeParsedTarget
                            {
                                Kind = QrCodeParsedKind.CodeTicket,
                                Code = userCode.Trim()
                            };
                        }

                        var idCandidate = segments.Count >= 2 ? segments[1] : (queryDict.TryGetValue("id", out var qId) ? qId : null);
                        if (!string.IsNullOrWhiteSpace(idCandidate))
                        {
                            if (long.TryParse(idCandidate.Trim(), out var legacyUserId) && legacyUserId > 0)
                            {
                                return new QrCodeParsedTarget
                                {
                                    Kind = QrCodeParsedKind.LegacyChatObjectId,
                                    LegacyChatObjectId = legacyUserId,
                                    IsGroupHint = false
                                };
                            }

                            return new QrCodeParsedTarget
                            {
                                Kind = QrCodeParsedKind.CodeTicket,
                                Code = idCandidate.Trim()
                            };
                        }
                    }

                    // 兼容 gotoim://group/join?ticket={code} 或 gotoim://group/{id}
                    if (first == "group")
                    {
                        if (queryDict.TryGetValue("ticket", out var grpTicket) && !string.IsNullOrWhiteSpace(grpTicket))
                        {
                            return new QrCodeParsedTarget
                            {
                                Kind = QrCodeParsedKind.CodeTicket,
                                Code = grpTicket.Trim(),
                                IsGroupHint = true
                            };
                        }

                        if (queryDict.TryGetValue("code", out var grpCode) && !string.IsNullOrWhiteSpace(grpCode))
                        {
                            return new QrCodeParsedTarget
                            {
                                Kind = QrCodeParsedKind.CodeTicket,
                                Code = grpCode.Trim(),
                                IsGroupHint = true
                            };
                        }

                        var groupSegment = segments.Count >= 2 ? segments[1] : (queryDict.TryGetValue("id", out var gId) ? gId : null);
                        if (!string.IsNullOrWhiteSpace(groupSegment) && !groupSegment.Equals("join", StringComparison.OrdinalIgnoreCase))
                        {
                            if (long.TryParse(groupSegment.Trim(), out var legacyGroupId) && legacyGroupId > 0)
                            {
                                return new QrCodeParsedTarget
                                {
                                    Kind = QrCodeParsedKind.LegacyChatObjectId,
                                    LegacyChatObjectId = legacyGroupId,
                                    IsGroupHint = true
                                };
                            }

                            return new QrCodeParsedTarget
                            {
                                Kind = QrCodeParsedKind.CodeTicket,
                                Code = groupSegment.Trim(),
                                IsGroupHint = true
                            };
                        }
                    }
                }
            }
        }

        return null;
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        var trimmed = query.TrimStart('?');
        foreach (var part in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = part.IndexOf('=');
            if (idx > 0)
            {
                var k = Uri.UnescapeDataString(part[..idx]);
                var v = Uri.UnescapeDataString(part[(idx + 1)..]);
                result[k] = v;
            }
        }

        return result;
    }
}
