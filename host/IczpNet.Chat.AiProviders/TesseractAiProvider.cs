using IczpNet.Ai.Tesseracts;
using IczpNet.Chat.Ai;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace IczpNet.Chat.AiProviders;

[Ai("Tesseract")]
public class TesseractAiProvider(
    ITesseractManager tesseractManager,
    IBlobManager blobManager) : AiProvider
{
    public ITesseractManager TesseractManager { get; } = tesseractManager;
    public IBlobManager BlobManager { get; } = blobManager;

    public override string GetModel() => "Github";

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

        if (message.MessageType != MessageTypes.Image)
        {
            var receiver = message.Receiver;

            await SendTextAsync(replySessionUnit, messageId, new TextContentInfo()
            {
                Text = $"请发送图片给我，我是{receiver.Name},\n账号：{receiver.Code}\n简介：{message.Receiver.Description}\n收到消息：{body}",
            });
            return;
        }

        var content = message.GetTypedContentEntity<ImageContent>();

        if (!content.BlobId.HasValue)
        {
            await SendTextAsync(replySessionUnit, messageId, new TextContentInfo()
            {
                Text = $"BlobId is null",
            });
            return;
        }

        var blob = await BlobManager.GetAsync(content.BlobId.Value);

        var fileBytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);

        var words = await TesseractManager.RecognizeAsync(fileBytes, "eng+chi_sim");

        if (words == null)
        {
            await SendTextAsync(replySessionUnit, messageId, new TextContentInfo()
            {
                Text = $"识别失败",
            });
            return;
        }
        var result = words.Select(x => x.Text).Where(x => !string.IsNullOrWhiteSpace(x)).JoinAsString(" ");

        await SendTextAsync(replySessionUnit, messageId, new TextContentInfo()
        {
            Text = $"识结果如下：\r\n {result}",
        });
    }
}
