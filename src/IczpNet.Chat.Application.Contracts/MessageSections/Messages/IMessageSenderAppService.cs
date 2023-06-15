using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace IczpNet.Chat.MessageSections.Messages
{
    public interface IMessageSenderAppService
    {
        Task<MessageInfo<TextContentInfo>> SendTextAsync(Guid sessionUnitId, MessageInput<TextContentInfo> input);
        Task<MessageInfo<CmdContentInfo>> SendCmdAsync(Guid sessionUnitId, MessageInput<CmdContentInfo> input);
        Task<MessageInfo<LinkContentInfo>> SendLinkAsync(Guid sessionUnitId, MessageInput<LinkContentInfo> input);
        Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(Guid sessionUnitId, MessageInput<HtmlContentInfo> input);
        Task<MessageInfo<ImageContentInfo>> SendImageAsync(Guid sessionUnitId, MessageInput<ImageContentInfo> input);
        Task<MessageInfo<SoundContentInfo>> SendSoundAsync(Guid sessionUnitId, MessageInput<SoundContentInfo> input);
        Task<MessageInfo<VideoContentInfo>> SendVideoAsync(Guid sessionUnitId, MessageInput<VideoContentInfo> input);
        Task<MessageInfo<FileContentInfo>> SendFileAsync(Guid sessionUnitId, MessageInput<FileContentInfo> input);
        Task<MessageInfo<LocationContentInfo>> SendLocationAsync(Guid sessionUnitId, MessageInput<LocationContentInfo> input);
        Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(Guid sessionUnitId, MessageInput<ContactsContentInfo> input);
        Task<MessageInfo<HistoryContentOutput>> SendHistoryAsync(Guid sessionUnitId, MessageInput<HistoryContentInput> input);
        Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(Guid sessionUnitId, MessageInput<RedEnvelopeContentInput> input);

        /// <summary>
        /// 转发消息
        /// </summary>
        /// <param name="sessionUnitId">当前会话单元</param>
        /// <param name="messageId">原消息</param>
        /// <param name="targets">目标</param>
        /// <returns></returns>
        Task<List<MessageDto>> ForwardAsync(Guid sessionUnitId, long messageId, List<Guid> targets);

        /// <summary>
        /// 消息撤回
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        Task<Dictionary<string, long>> RollbackAsync(long messageId);
    }
}
