using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Security;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Xunit;

namespace IczpNet.Chat.MessageSections.Tests;

public class MessageValidator_Tests
{
    private class FakeUrlSecurityManager : IUrlSecurityManager
    {
        public Task<UrlSecurityEvaluationResult> CheckUrlAsync(string url)
        {
            if (url.Contains("blocked-phishing.com") || url.Contains("malicious.org"))
            {
                return Task.FromResult(new UrlSecurityEvaluationResult
                {
                    Url = url,
                    Domain = "blocked-phishing.com",
                    Level = UrlSecurityLevel.Blocked,
                    Category = UrlRiskCategory.Phishing,
                    Message = "包含违规钓鱼网址",
                    InterceptReason = "平台黑名单"
                });
            }

            if (url.Contains("warning-site.com"))
            {
                return Task.FromResult(new UrlSecurityEvaluationResult
                {
                    Url = url,
                    Domain = "warning-site.com",
                    Level = UrlSecurityLevel.Warning,
                    Category = UrlRiskCategory.UnknownThirdParty,
                    Message = "未知第三方链接"
                });
            }

            return Task.FromResult(new UrlSecurityEvaluationResult
            {
                Url = url,
                Domain = "example.com",
                Level = UrlSecurityLevel.Safe,
                Category = UrlRiskCategory.OfficialOrWhitelisted,
                Message = "安全链接"
            });
        }

        public Task<SecurityReport> SubmitReportAsync(
            string target,
            string targetType,
            string reason,
            string description = null,
            long? messageId = null,
            long? reporterChatObjectId = null,
            Guid? reporterSessionUnitId = null)
        {
            throw new NotImplementedException();
        }

        public Task RefreshCacheAsync()
        {
            return Task.CompletedTask;
        }
    }

    private class TestMessage : Message
    {
        public TestMessage() : base() { }
    }

    [Fact]
    public async Task CheckAsync_Passes_When_ContentJson_Is_Null_Or_Empty()
    {
        var validator = new MessageValidator(new FakeUrlSecurityManager());
        var message = new TestMessage();

        await validator.CheckAsync(message);
    }

    [Fact]
    public async Task CheckAsync_Passes_When_Urls_Are_Safe_Or_Warning()
    {
        var validator = new MessageValidator(new FakeUrlSecurityManager());
        var message = new TestMessage();
        message.SetContentJson("{\"text\":\"Please visit https://example.com and check https://warning-site.com/doc.\"}");

        await validator.CheckAsync(message);
    }

    [Fact]
    public async Task CheckAsync_Throws_BusinessException_When_Url_Is_Blocked()
    {
        var validator = new MessageValidator(new FakeUrlSecurityManager());
        var message = new TestMessage();
        message.SetContentJson("{\"text\":\"Click here to get prize: https://blocked-phishing.com/login.html now!\"}");

        var ex = await Assert.ThrowsAsync<BusinessException>(() => validator.CheckAsync(message));
        Assert.Equal(ChatErrorCodes.Security.MessageContainsProhibitedUrl, ex.Code);
        Assert.True(ex.Data.Contains("Url"));
        Assert.Equal("https://blocked-phishing.com/login.html", ex.Data["Url"]);
    }
}
