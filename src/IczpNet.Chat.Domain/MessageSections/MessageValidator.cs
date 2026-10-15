using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Security;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.MessageSections;

public class MessageValidator : DomainService, IMessageValidator
{
    protected IUrlSecurityManager UrlSecurityManager { get; }

    private static readonly Regex UrlRegex = new(
        @"https?://[^\s<>""'{}|\\^`]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(1));

    public MessageValidator(IUrlSecurityManager urlSecurityManager)
    {
        UrlSecurityManager = urlSecurityManager;
    }

    public virtual async Task CheckAsync(Message entity)
    {
        if (entity == null || string.IsNullOrWhiteSpace(entity.ContentJson))
        {
            return;
        }

        var content = entity.ContentJson.Replace("\\/", "/");
        var matches = UrlRegex.Matches(content);
        if (matches.Count == 0)
        {
            return;
        }

        var checkedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in matches)
        {
            var rawUrl = match.Value.TrimEnd('.', ',', ';', '!', '?', ')', ']');
            if (!checkedUrls.Add(rawUrl))
            {
                continue;
            }

            var checkResult = await UrlSecurityManager.CheckUrlAsync(rawUrl);
            if (checkResult != null && checkResult.Level == UrlSecurityLevel.Blocked)
            {
                throw new BusinessException(ChatErrorCodes.Security.MessageContainsProhibitedUrl)
                    .WithData("Url", rawUrl)
                    .WithData("Reason", checkResult.InterceptReason ?? checkResult.Message);
            }
        }
    }
}

