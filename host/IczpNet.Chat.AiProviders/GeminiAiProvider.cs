using GenerativeAI.Types;
using IczpNet.Ai;
using IczpNet.Ai.Geminis;
using IczpNet.Chat.Ai;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace IczpNet.Chat.AiProviders;

[Ai("Gemini")]
public class GeminiAiProvider(
    IGeminiManager geminiManager,
    IBlobManager blobManager) : AiProvider
{
    public IGeminiManager GeminiManager { get; } = geminiManager;
    public IBlobManager BlobManager { get; } = blobManager;

    public override string GetModel() => "Gemini 2.0";

    [UnitOfWork]
    public override async Task HandleAsync(long messageId)
    {
        var message = await MessageRepository.GetAsync(messageId);

        if (message.IsRollbackMessage())
        {
            Logger.LogInformation($"Message is rollback:{message}");
            return;
        }

        var replySessionUnit = await SessionUnitManager.FindCacheAsync(message.ReceiverId.Value, message.SenderId.Value);

        if (replySessionUnit == null)
        {
            Logger.LogWarning($"会话不存在:[{message.ReceiverId},{message.SenderId}]");
            return;
        }

        var body = message.GetTypedContentEntity()?.GetBody();

        Logger.LogInformation($"收到消息：{body}");

        var history = new List<InputContent>() {
            new InputContent {
                 Role = nameof(message.Sender.Name),
                 Parts =$"我的名字是{message.Sender.Name}",
            },
            new InputContent {
                 Role = nameof(message.Sender.Gender),
                 Parts =$"我的性别是{message.Sender.Gender}",
            },
            new InputContent {
                 Role = nameof(message.Sender.Id),
                 Parts =$"我的ID是{message.Sender.Id}",
            },
            new InputContent {
                 Role = nameof(message.Sender.CreationTime),
                 Parts =$"我的创建时间是{message.Sender.CreationTime}",
            },
        };



        byte[] fileBytes = null;
        string fileName = null;

        async Task ApplyMessageAsync(Message message)
        {
            if (message == null)
            {
                return;
            }
            var quoteContent = message.GetTypedContentEntity();

            if (typeof(IBlobId).IsAssignableFrom(quoteContent.GetType()))
            {
                var blobId = (quoteContent as IBlobId).BlobId;
                if (blobId.HasValue)
                {
                    var blob = await BlobManager.GetAsync(blobId.Value);
                    fileName = blob.Name;
                    fileBytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);
                }
            }
            switch (message.MessageType)
            {
                case MessageTypes.Cmd:
                case MessageTypes.Html:
                case MessageTypes.Article:
                case MessageTypes.Text:
                case MessageTypes.Contacts:
                    if (quoteContent != null)
                    {
                        history.Add(new InputContent()
                        {
                            Role = "quoteMessage",
                            Parts = quoteContent?.GetBody(),
                        });
                    }
                    break;
                case MessageTypes.Image:
                case MessageTypes.Sound:
                case MessageTypes.Video:
                case MessageTypes.File:
                    break;
                case MessageTypes.Link:
                    break;
                case MessageTypes.Location:
                    break;
                case MessageTypes.RedEnvelope:
                    break;
                case MessageTypes.History:
                    break;
            }
        }

        //引用消息
        await ApplyMessageAsync(message.QuoteMessage);

        await ApplyMessageAsync(message);

        AiOutput result;

        if (fileBytes != null)
        {
            result = await GeminiManager.GenerateContentAsync(body, fileBytes, fileName);
        }
        else
        {
            result = await GeminiManager.GenerateTextAsync($"{message.SessionId}", body, [.. history]);
        }

        await SendTextAsync(replySessionUnit, messageId, new TextContentInfo()
        {
            Text = $"{result.Data}\r\n用时：{(result.ExecutionDaration ?? 0) / 1000:F1}s",
        });
    }
}
