using IczpNet.AbpCommons;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using IczpNet.Chat.SessionSections.SessionUnits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.MessageSections
{
    public class MessageSender : DomainService, IMessageSender
    {
        protected IObjectMapper ObjectMapper => LazyServiceProvider.LazyGetRequiredService<IObjectMapper>();
        protected IMessageRepository Repository => LazyServiceProvider.LazyGetRequiredService<IMessageRepository>();
        protected IRedEnvelopeGenerator RedEnvelopeGenerator => LazyServiceProvider.LazyGetRequiredService<IRedEnvelopeGenerator>();
        protected IContentResolver ContentResolver => LazyServiceProvider.LazyGetRequiredService<IContentResolver>();
        protected IMessageManager MessageManager => LazyServiceProvider.LazyGetRequiredService<IMessageManager>();
        protected IRepository<RedEnvelopeContent, Guid> RedEnvelopeContentRepository { get; }
        protected IRedPacketManager RedPacketManager { get; }

        public MessageSender(
            IRepository<RedEnvelopeContent, Guid> redEnvelopeContentRepository, IRedPacketManager redPacketManager)
        {
            RedEnvelopeContentRepository = redEnvelopeContentRepository;
            RedPacketManager = redPacketManager;
        }

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

        public Task<MessageInfo<TextContentInfo>> SendTextAsync(SessionUnit senderSessionUnit, MessageInput<TextContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return MessageManager.SendAsync<TextContentInfo, TextContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public Task<MessageInfo<CmdContentInfo>> SendCmdAsync(SessionUnit senderSessionUnit, MessageInput<CmdContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return MessageManager.SendAsync<CmdContentInfo, CmdContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public Task<MessageInfo<LinkContentInfo>> SendLinkAsync(SessionUnit senderSessionUnit, MessageInput<LinkContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return MessageManager.SendAsync<LinkContentInfo, LinkContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(SessionUnit senderSessionUnit, MessageInput<HtmlContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return await MessageManager.SendAsync<HtmlContentInfo, HtmlContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<ImageContentInfo>> SendImageAsync(SessionUnit senderSessionUnit, MessageInput<ImageContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return await MessageManager.SendAsync<ImageContentInfo, ImageContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<SoundContentInfo>> SendSoundAsync(SessionUnit senderSessionUnit, MessageInput<SoundContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return await MessageManager.SendAsync<SoundContentInfo, SoundContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<VideoContentInfo>> SendVideoAsync(SessionUnit senderSessionUnit, MessageInput<VideoContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return await MessageManager.SendAsync<VideoContentInfo, VideoContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<FileContentInfo>> SendFileAsync(SessionUnit senderSessionUnit, MessageInput<FileContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return await MessageManager.SendAsync<FileContentInfo, FileContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<LocationContentInfo>> SendLocationAsync(SessionUnit senderSessionUnit, MessageInput<LocationContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return await MessageManager.SendAsync<LocationContentInfo, LocationContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(SessionUnit senderSessionUnit, MessageInput<ContactsContentInfo> input, SessionUnit receiverSessionUnit = null)
        {
            return await MessageManager.SendAsync<ContactsContentInfo, ContactsContent>(senderSessionUnit, input, receiverSessionUnit);
        }

        public virtual async Task<MessageInfo<HistoryContentOutput>> SendHistoryAsync(SessionUnit senderSessionUnit, MessageInput<HistoryContentInput> input, SessionUnit receiverSessionUnit = null)
        {
            var messageIdList = input.Content.MessageIdList;

            var selectedMessageList = (await Repository.GetQueryableAsync())
                .Where(x => messageIdList.Contains(x.Id))
                .Where(x => !x.IsRollbacked)
                .OrderBy(x => x.Id)
            //.Select(x => x.Id)
                .ToList();
            var description = selectedMessageList.Take(3)
                .Select(x => $"{x.Sender?.Name}:{x.GetTypedContentEntity()?.GetBody()}")
                .ToArray()
                .JoinAsString("\n");

            var messageContent = new HistoryContent(GuidGenerator.Create(), $"聊天记录({selectedMessageList.Count})", description);

            return await MessageManager.SendAsync<HistoryContentOutput, HistoryContent>(senderSessionUnit, input, messageContent, receiverSessionUnit);
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

        public virtual async Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(SessionUnit senderSessionUnit, MessageInput<RedEnvelopeContentInput> input, SessionUnit receiverSessionUnit = null)
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

            return await MessageManager.SendAsync<RedEnvelopeContentOutput, RedEnvelopeContent>(senderSessionUnit, input, messageContent, receiverSessionUnit);
        }

    }
}
