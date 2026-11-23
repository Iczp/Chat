using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.ChatObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.MessageSections;

public class MessageSender(
    IRepository<RedEnvelopeContent, Guid> redEnvelopeContentRepository,
    IRedPacketManager redPacketManager) : DomainService, IMessageSender
{
    protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();
    protected IMessageRepository Repository => LazyServiceProvider.LazyGetRequiredService<IMessageRepository>();
    protected IRedEnvelopeGenerator RedEnvelopeGenerator => LazyServiceProvider.LazyGetRequiredService<IRedEnvelopeGenerator>();
    protected IContentResolver ContentResolver => LazyServiceProvider.LazyGetRequiredService<IContentResolver>();
    protected IMessageManager MessageManager => LazyServiceProvider.LazyGetRequiredService<IMessageManager>();
    protected IRepository<RedEnvelopeContent, Guid> RedEnvelopeContentRepository { get; } = redEnvelopeContentRepository;
    protected IRedPacketManager RedPacketManager { get; } = redPacketManager;

    protected virtual IContentProvider GetContentProvider(MessageTypes messageType)
    {
        var providerType = ContentResolver.GetProviderTypeOrDefault(messageType);
        return LazyServiceProvider.LazyGetService(providerType) as IContentProvider;
    }

    //public virtual async Task<MessageInfo<TextContentInfo>> SendTextTemplateMessageAsync(MessageInput<TextContentInfo> input)
    //{
    //    var provider = GetContentProvider(TextContentProvider.Name);
    //    var redEnvelope = await provider.CreateAsync<TextContentInfo, TextContent>(input.Content);
    //    return await MessageManager.SendMessageAsync<TextContentInfo>(input, async x => await Task.FromResult(redEnvelope));
    //}

    public Task<MessageInfo<TextContentInfo>> SendTextAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<TextContentInfo> input)
    {
        return MessageManager.SendAsync<TextContentInfo, TextContent>(senderSessionUnit, input);
    }

    public Task<MessageInfo<CmdContentInfo>> SendCmdAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<CmdContentInfo> input)
    {
        return MessageManager.SendAsync<CmdContentInfo, CmdContent>(senderSessionUnit, input);
    }

    public Task<MessageInfo<LinkContentInfo>> SendLinkAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<LinkContentInfo> input)
    {
        return MessageManager.SendAsync<LinkContentInfo, LinkContent>(senderSessionUnit, input);
    }

    public async Task<MessageInfo<ArticleContentInfo>> SendArticleContentMessageAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<ArticleContentInput> input)
    {
        Assert.If(senderSessionUnit.OwnerObjectType != ChatObjectTypeEnums.Official && senderSessionUnit.OwnerObjectType != ChatObjectTypeEnums.Subscription,
            "Article messages require an official or subscription account.");
        var content = ArticleContent.Create(input?.Content);
        var account = await LazyServiceProvider.LazyGetRequiredService<IChatObjectManager>().GetItemByCacheAsync(senderSessionUnit.OwnerId);
        Assert.If(account == null || !account.IsEnabled || (account.ObjectType != ChatObjectTypeEnums.Official && account.ObjectType != ChatObjectTypeEnums.Subscription),
            "The article sender account is disabled or invalid.");
        return await MessageManager.SendAsync<ArticleContentInfo, ArticleContent>(senderSessionUnit, input, content);
    }

    public virtual async Task<MessageInfo<TemplateContentInfo>> SendTemplateContentMessageAsync(
        SessionUnitCacheItem senderSessionUnit,
        MessageInput<TemplateContentInput> input)
    {
        Assert.NotNull(senderSessionUnit, "Sender session unit required.");
        Assert.NotNull(input, "Message input required.");
        Assert.NotNull(input.Content, "TemplateContentInput required.");

        var templateId = input.Content.TemplateId;
        var templateVersion = input.Content.TemplateVersion;
        var data = input.Content.Data;

        var templateRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageTemplateDefinition, Guid>>();
        var revisionRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<MessageTemplateRevision, Guid>>();
        var templateManager = LazyServiceProvider.LazyGetRequiredService<MessageTemplateManager>();

        var message = await MessageManager.CreateMessageAsync(
            senderSessionUnit,
            async (msg) =>
            {
                var template = await templateRepository.FindAsync(templateId);
                if (template == null)
                {
                    throw new UserFriendlyException("模板不存在。", ChatErrorCodes.TemplateMessages.TemplateNotFound);
                }
                if (!template.IsEnabled)
                {
                    throw new UserFriendlyException("模板已被停用，禁止发送。", ChatErrorCodes.TemplateMessages.TemplateDisabled);
                }
                if (!template.PublishedVersion.HasValue || template.PublishedVersion.Value < templateVersion)
                {
                    throw new UserFriendlyException($"模板版本 {templateVersion} 尚未发布或不存在。", ChatErrorCodes.TemplateMessages.TemplateVersionNotPublished);
                }

                // Check permission: if private, must match sender's OwnerId
                if (template.Visibility == MessageTemplateVisibility.Private && template.OwnerId != senderSessionUnit.OwnerId)
                {
                    throw new UserFriendlyException("无权使用该私有模板。", ChatErrorCodes.TemplateMessages.TemplateForbidden);
                }

                var revision = await (await revisionRepository.GetQueryableAsync())
                    .FirstOrDefaultAsync(x => x.TemplateId == templateId && x.Version == templateVersion);

                if (revision == null)
                {
                    throw new UserFriendlyException($"模板发布版本 {templateVersion} 不存在。", ChatErrorCodes.TemplateMessages.TemplateVersionNotPublished);
                }

                var parsed = templateManager.ValidateDefinitionJson(revision.DefinitionJson);
                var sanitizedData = templateManager.ValidateAndSanitizeData(data, parsed);
                var fallbackText = templateManager.GenerateFallbackText(parsed, sanitizedData);
                var snapshot = templateManager.BuildSnapshot(templateId, templateVersion, parsed, sanitizedData, fallbackText);
                var snapshotJson = System.Text.Json.JsonSerializer.Serialize(snapshot);

                var templateContent = new TemplateContent(
                    templateId,
                    templateVersion,
                    MessageTemplateConsts.SchemaVersion,
                    fallbackText,
                    snapshotJson,
                    template.TenantId);

                return templateContent;
            },
            clientMessageId: input.ClientMessageId,
            quoteMessageId: input.QuoteMessageId,
            remindList: input.RemindList,
            receiverSessionUnitId: input.ReceiverSessionUnitId);

        var output = ObjectMapper.Map<Message, MessageInfo<TemplateContentInfo>>(message);
        var sessionUnitManager = LazyServiceProvider.LazyGetRequiredService<ISessionUnitManager>();
        output.SenderSessionUnit ??= await sessionUnitManager.MapToSenderAsync(senderSessionUnit);
        return output;
    }

    public virtual async Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<HtmlContentInfo> input)
    {
        return await MessageManager.SendAsync<HtmlContentInfo, HtmlContent>(senderSessionUnit, input);
    }

    public virtual async Task<MessageInfo<ImageContentInfo>> SendImageAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<ImageContentInfo> input)
    {
        return await MessageManager.SendAsync<ImageContentInfo, ImageContent>(senderSessionUnit, input);
    }

    public virtual async Task<MessageInfo<SoundContentInfo>> SendSoundAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<SoundContentInfo> input)
    {
        return await MessageManager.SendAsync<SoundContentInfo, SoundContent>(senderSessionUnit, input);
    }

    public virtual async Task<MessageInfo<VideoContentInfo>> SendVideoAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<VideoContentInfo> input)
    {
        return await MessageManager.SendAsync<VideoContentInfo, VideoContent>(senderSessionUnit, input);
    }

    public virtual async Task<MessageInfo<FileContentInfo>> SendFileAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<FileContentInfo> input)
    {
        return await MessageManager.SendAsync<FileContentInfo, FileContent>(senderSessionUnit, input);
    }

    public virtual async Task<MessageInfo<LocationContentInfo>> SendLocationAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<LocationContentInfo> input)
    {
        return await MessageManager.SendAsync<LocationContentInfo, LocationContent>(senderSessionUnit, input);
    }

    public virtual async Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<ContactsContentInfo> input)
    {
        return await MessageManager.SendAsync<ContactsContentInfo, ContactsContent>(senderSessionUnit, input);
    }

    public virtual async Task<MessageInfo<HistoryContentOutput>> SendHistoryAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<HistoryContentInput> input)
    {
        var messageIdList = input.Content.MessageIdList.Distinct().ToList();
        Assert.If(messageIdList.Count == 0 || messageIdList.Count > 100, "Select between 1 and 100 messages.");

        var selectedMessageList = (await Repository.GetQueryableAsync())
            .Where(x => messageIdList.Contains(x.Id))
            .Where(new IczpNet.Chat.Specifications.OwnerMessageSpecification(senderSessionUnit.OwnerId).ToExpression())
            .Where(x => !x.IsPrivate)
            .Where(x => !x.IsRollbacked)
            .OrderBy(x => x.Id)
        //.Select(x => x.Id)
            .ToList();
        Assert.If(selectedMessageList.Count != messageIdList.Count, "Some selected messages are not visible or cannot be forwarded.");
        var description = selectedMessageList.Take(3)
            .Select(x => $"{x.Sender?.Name}:{x.GetTypedContentEntity()?.GetBody()}")
            .ToArray()
            .JoinAsString("\n");

        var messageContent = new HistoryContent(GuidGenerator.Create(), $"聊天记录({selectedMessageList.Count})", description);
        messageContent.SetHistoryMessageList(selectedMessageList.Select(x => new HistoryMessage(messageContent, x)).ToList());

        var result = await MessageManager.SendAsync<HistoryContentOutput, HistoryContent>(senderSessionUnit, input, messageContent);
        await MessageManager.BindHistoryAttachmentsAsync(result.Id, senderSessionUnit.SessionId, messageIdList);
        return result;
    }

    protected virtual async Task CheckRedEnvelopeAsync(RedEnvelopeContentInput redEnvelope)
    {
        Assert.If(redEnvelope.Count <= 0, $"红包数量出错:{redEnvelope.Count}");

        Assert.If(redEnvelope.TotalAmount <= 0, $"红包总金额出错:{redEnvelope.TotalAmount}");

        if (redEnvelope.GrantMode == GrantModes.FixedAmount)
        {
            Assert.If(redEnvelope.Amount < 0.01m, $"单个金额不能小于1分钱:{redEnvelope.Amount}");

            Assert.If(redEnvelope.TotalAmount != redEnvelope.Amount * redEnvelope.Count, $"定额红包总金额出错:{redEnvelope.TotalAmount}!=${redEnvelope.Amount * redEnvelope.Count}");
        }

        if (redEnvelope.GrantMode == GrantModes.RandomAmount)
        {
            Assert.If(redEnvelope.TotalAmount != redEnvelope.Amount, $"拼手气红包总金额与单个金额不相等");

            Assert.If(redEnvelope.TotalAmount / redEnvelope.Count < 0.01m, $"红包最小金额不能小于1分钱");
        }

        await Task.Yield();
    }

    public virtual async Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(SessionUnitCacheItem senderSessionUnit, MessageInput<RedEnvelopeContentInput> input)
    {
        var redEnvelope = input.Content;

        await CheckRedEnvelopeAsync(redEnvelope);

        var messageContent = await RedPacketManager.CreateAsync(
             ownerId: senderSessionUnit.OwnerId,
             grantMode: redEnvelope.GrantMode,
             amount: redEnvelope.Amount,
             count: redEnvelope.Count,
             totalAmount: redEnvelope.TotalAmount,
             text: redEnvelope.Text);

        return await MessageManager.SendAsync<RedEnvelopeContentOutput, RedEnvelopeContent>(senderSessionUnit, input, messageContent);
    }

}
