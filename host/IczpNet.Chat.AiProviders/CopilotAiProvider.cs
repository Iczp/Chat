using IczpNet.Chat.Ai;
using IczpNet.Chat.Blobs;
using IczpNet.Chat.MessageSections.Templates;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Uow;

namespace IczpNet.Chat.AiProviders;

[Ai("Copilot")]
public class CopilotAiProvider(
    IBlobManager blobManager) : AiProvider
{
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

        var receiver = message.Receiver;

        string result = $"我是{receiver.Name},\n账号：{receiver.Code}\n简介：{message.Receiver.Description}\n收到消息：{body}";

        await SendTextAsync(replySessionUnit, messageId, new TextContentInfo()
        {
            Text = result,
        });
    }
}
