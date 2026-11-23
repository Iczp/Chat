using IczpNet.BizCrypts;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.Enums;
using IczpNet.Chat.Follows;
using IczpNet.Chat.HttpRequests;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.Json;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;

namespace IczpNet.Chat.Developers;

public class DeveloperJob(
    IMessageSender messageSender,
    ISessionUnitManager sessionUnitManager,
    IFollowManager followManager,
    IMessageRepository messageRepository,
    IJsonSerializer jsonSerializer,
    IBackgroundJobManager backgroundJobManager,
    IChatObjectManager chatObjectManager,
    IDeveloperManager developerManager,
    IHttpRequestManager httpRequestManager) : DomainService, IAsyncBackgroundJob<DeveloperJobArg>, ITransientDependency
{
    protected IMessageSender MessageSender { get; } = messageSender;
    protected ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    protected IFollowManager FollowManager { get; } = followManager;
    protected IJsonSerializer JsonSerializer { get; } = jsonSerializer;
    protected IBackgroundJobManager BackgroundJobManager { get; } = backgroundJobManager;
    protected IDeveloperManager DeveloperManager { get; } = developerManager;
    protected IChatObjectManager ChatObjectManager { get; } = chatObjectManager;
    protected IMessageRepository MessageRepository { get; } = messageRepository;
    protected IHttpRequestManager HttpRequestManager { get; } = httpRequestManager;

    protected Type ObjectMapperContext { get; set; }
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetService<IObjectMapper>(provider =>
        ObjectMapperContext == null
            ? provider.GetRequiredService<IObjectMapper>()
            : (IObjectMapper)provider.GetRequiredService(typeof(IObjectMapper<>).MakeGenericType(ObjectMapperContext)));

    [UnitOfWork]
    public async Task ExecuteAsync(DeveloperJobArg args)
    {
        Logger.LogInformation($"{nameof(DeveloperJob)} is executed:{args}");

        var message = await MessageRepository.GetAsync(args.MessageId);

        if (message.IsRollbackMessage())
        {
            Logger.LogInformation($"Message is rollback:{message}");
            return;
        }

        if (!message.ReceiverId.HasValue || !message.SenderId.HasValue)
        {
            Logger.LogInformation($"Message ReceiverId or SenderId is null: {message.Id}");
            return;
        }

        var receiver = message.Receiver ?? await ChatObjectManager.GetAsync(message.ReceiverId.Value);
        var developer = receiver.Developer;

        if (developer == null || !developer.IsEnabled || string.IsNullOrWhiteSpace(developer.PostUrl))
        {
            Logger.LogInformation($"Receiver Developer is null or not enabled: ReceiverId={message.ReceiverId}");
            return;
        }

        var content = message.ContentJson;
        if (message.MessageType == MessageTypes.Text && !string.IsNullOrWhiteSpace(message.ContentJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(message.ContentJson);
                if (doc.RootElement.TryGetProperty("text", out var t) || doc.RootElement.TryGetProperty("Text", out t))
                {
                    content = t.GetString();
                }
            }
            catch { }
        }

        var payload = JsonSerializer.Serialize(new
        {
            ToUserName = receiver.Code ?? receiver.Id.ToString(),
            FromUserName = message.Sender?.Code ?? message.SenderId.ToString(),
            CreateTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            MsgType = message.MessageType.ToString().ToLower(),
            Content = content,
            MsgId = message.Id.ToString(),
            SessionUnitId = message.SenderSessionUnitId?.ToString()
        });

        var bizCrypt = new BizCrypt(developer.Token, developer.EncodingAesKey, developer.OwnerId.ToString());
        var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var nonce = BizCrypt.CreateRandCode(10, "123456789");
        var echo = bizCrypt.Encrypt(payload);
        var signature = BizCrypt.GenerateSignature(developer.Token, timeStamp, nonce, echo);

        var remoteUrl = developer.PostUrl + (developer.PostUrl.Contains("?") ? "&" : "?") +
            $"signature={signature}&timestamp={timeStamp}&nonce={nonce}";

        var jsonBody = JsonSerializer.Serialize(new { encrypt = echo });
        var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var req = await HttpRequestManager.RequestAsync(HttpMethod.Post, remoteUrl, httpContent, name: nameof(DeveloperJob));
        Logger.LogInformation($"DeveloperJob Webhook POST ReqId={req.Id}, StatusCode={req.StatusCode}, url={req.Url}");

        if (req.IsSuccess && !string.IsNullOrWhiteSpace(req.Response?.Content))
        {
            var rawBody = req.Response.Content.Trim();
            if (!string.Equals(rawBody, "success", StringComparison.OrdinalIgnoreCase))
            {
                var replyText = TryExtractReplyText(rawBody, bizCrypt);
                if (!string.IsNullOrWhiteSpace(replyText))
                {
                    var replySessionUnit = await SessionUnitManager.FindCacheAsync(message.ReceiverId.Value, message.SenderId.Value);
                    if (replySessionUnit != null)
                    {
                        await MessageSender.SendTextAsync(replySessionUnit, new MessageInput<TextContentInfo>
                        {
                            QuoteMessageId = message.Id,
                            Content = new TextContentInfo
                            {
                                Text = replyText
                            }
                        });
                        Logger.LogInformation($"DeveloperJob passive reply sent for MessageId={message.Id}");
                    }
                }
            }
        }
    }

    protected virtual string TryExtractReplyText(string rawBody, BizCrypt bizCrypt)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawBody);
            if (doc.RootElement.TryGetProperty("encrypt", out var encryptProp))
            {
                var cipher = encryptProp.GetString();
                if (!string.IsNullOrWhiteSpace(cipher))
                {
                    var decrypted = bizCrypt.Decrypt(cipher);
                    return ExtractFromContentOrRaw(decrypted);
                }
            }
            else if (doc.RootElement.TryGetProperty("Content", out var contentProp) ||
                     doc.RootElement.TryGetProperty("content", out contentProp))
            {
                return contentProp.GetString();
            }
        }
        catch
        {
            try
            {
                var decrypted = bizCrypt.Decrypt(rawBody);
                return ExtractFromContentOrRaw(decrypted);
            }
            catch
            {
                return rawBody;
            }
        }
        return rawBody;
    }

    private static string ExtractFromContentOrRaw(string decrypted)
    {
        if (string.IsNullOrWhiteSpace(decrypted)) return null;
        try
        {
            using var doc = JsonDocument.Parse(decrypted);
            if (doc.RootElement.TryGetProperty("Content", out var contentProp) ||
                doc.RootElement.TryGetProperty("content", out contentProp))
            {
                return contentProp.GetString();
            }
        }
        catch { }
        return decrypted;
    }
}
