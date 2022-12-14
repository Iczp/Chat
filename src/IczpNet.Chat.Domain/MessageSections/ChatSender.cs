using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.MessageSections
{
    public class ChatSender : DomainService, IChatSender
    {
        protected IObjectMapper ObjectMapper { get; }
        protected IRepository<Message, Guid> Repository { get; }
        protected IRedEnvelopeGenerator RedEnvelopeGenerator { get; }
        protected IContentResolver ContentResolver { get; }
        protected IMessageManager MessageManager { get; }

        public ChatSender(
            IMessageManager messageManager,
            IRedEnvelopeGenerator redEnvelopeGenerator,
            IContentResolver contentResolver,
            IRepository<Message, Guid> repository,
            IObjectMapper objectMapper)
        {
            MessageManager = messageManager;
            RedEnvelopeGenerator = redEnvelopeGenerator;
            ContentResolver = contentResolver;
            Repository = repository;
            ObjectMapper = objectMapper;
        }

        public virtual Task<MessageInfo<CmdContentInfo>> SendCmdMessageAsync(MessageInput<CmdContentInfo> input)
        {
            return MessageManager.SendMessageAsync<CmdContentInfo>(input, async x => await Task.FromResult(ObjectMapper.Map<CmdContentInfo, CmdContent>(input.Content)));
        }

        public virtual async Task<MessageInfo<TextContentInfo>> SendTextMessageAsync(MessageInput<TextContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<TextContentInfo, TextContent>(input.Content);
            return await MessageManager.SendMessageAsync<TextContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<HtmlContentInfo>> SendHtmlMessageAsync(MessageInput<HtmlContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<HtmlContentInfo, HtmlContent>(input.Content);
            return await MessageManager.SendMessageAsync<HtmlContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<ImageContentInfo>> SendImageMessageAsync(MessageInput<ImageContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<ImageContentInfo, ImageContent>(input.Content);
            return await MessageManager.SendMessageAsync<ImageContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<SoundContentInfo>> SendSoundMessageAsync(MessageInput<SoundContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<SoundContentInfo, SoundContent>(input.Content);
            return await MessageManager.SendMessageAsync<SoundContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<VideoContentInfo>> SendVideoMessageAsync(MessageInput<VideoContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<VideoContentInfo, VideoContent>(input.Content);
            return await MessageManager.SendMessageAsync<VideoContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<FileContentInfo>> SendFileMessageAsync(MessageInput<FileContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<FileContentInfo, FileContent>(input.Content);
            return await MessageManager.SendMessageAsync<FileContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<LocationContentInfo>> SendLocationMessageAsync(MessageInput<LocationContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<LocationContentInfo, LocationContent>(input.Content);
            return await MessageManager.SendMessageAsync<LocationContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<ContactsContentInfo>> SendContactsMessageAsync(MessageInput<ContactsContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<ContactsContentInfo, ContactsContent>(input.Content);
            return await MessageManager.SendMessageAsync<ContactsContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<LinkContentInfo>> SendLinkMessageAsync(MessageInput<LinkContentInfo> input)
        {
            var messageContent = ObjectMapper.Map<LinkContentInfo, LinkContent>(input.Content);
            return await MessageManager.SendMessageAsync<LinkContentInfo>(input, async x => await Task.FromResult(messageContent));
        }

        public virtual async Task<MessageInfo<HistoryContentOutput>> SendHistoryMessageAsync(MessageInput<HistoryContentInput> input)
        {
            var messageIdList = input.Content.MessageIdList;

            var selectedMessageList = (await Repository.GetQueryableAsync())
                .Where(x => messageIdList.Contains(x.Id))
                .Where(x => !x.IsRollbacked || x.RollbackTime == null)
                .OrderBy(x => x.AutoId)
            //.Select(x => x.Id)
                .ToList();
            var description = selectedMessageList.Take(3)
                .Select(x => $"{x.Sender?.Name}:{x.GetTypedContent()?.GetBody()}")
                .ToArray()
                .JoinAsString("\n");

            var messageContent = new HistoryContent(GuidGenerator.Create(), $"聊天记录({selectedMessageList.Count})", description);

            return await MessageManager.SendMessageAsync<HistoryContentOutput>(input, async message =>
            {
                var historyDetailList = selectedMessageList.Select(x => new HistoryMessage(messageContent, message)).ToList();

                messageContent.SetHistoryMessageList(historyDetailList);

                //message.HistoryContentList.Add(historyContent);
                return await Task.FromResult(messageContent);
            });
        }

        public virtual async Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeMessageAsync(MessageInput<RedEnvelopeContentInput> input)
        {
            var content = input.Content;
            var messageContent = new RedEnvelopeContent(
                id: GuidGenerator.Create(),
                grantMode: content.GrantMode,
                amount: content.Amount,
                count: content.Count,
                totalAmount: content.TotalAmount,
                text: content.Text
                );

            var redEnvelopeUnitList = await RedEnvelopeGenerator.MakeAsync(content.GrantMode, messageContent.Id, content.Amount, content.Count, content.TotalAmount);

            messageContent.SetRedEnvelopeUnitList(redEnvelopeUnitList);

            return await MessageManager.SendMessageAsync<RedEnvelopeContentOutput>(input, async x => await Task.FromResult(messageContent));
        }

        protected virtual IContentProvider GetContentProvider(string providerName)
        {
            var providerType = ContentResolver.GetProviderTypeOrDefault(providerName);
            return LazyServiceProvider.LazyGetService(providerType) as IContentProvider;
        }

        public virtual async Task<MessageInfo<TextContentInfo>> SendTextTemplateMessageAsync(MessageInput<TextContentInfo> input)
        {
            var provider = GetContentProvider(TextContentProvider.Name);
            var content = await provider.Create<TextContentInfo, TextContent>(input.Content);
            return await MessageManager.SendMessageAsync<TextContentInfo>(input, async x => await Task.FromResult(content));
        }
    }
}
