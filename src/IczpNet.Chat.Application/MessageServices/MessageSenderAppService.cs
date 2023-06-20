using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Messages.Dtos;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionSections.SessionUnits;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageServices;

/// <summary>
/// 消息发送器
/// </summary>
public class MessageSenderAppService : ChatAppService, IMessageSenderAppService
{
    protected IMessageRepository Repository { get; }
    protected IMessageSender MessageSender { get; }
    protected IMessageManager MessageManager { get; }

    public MessageSenderAppService(
        IMessageRepository repository,
        IMessageManager messageManager,
        IMessageSender messageSender) 
    {
        Repository = repository;
        MessageSender = messageSender;
        MessageManager = messageManager;
    }

    protected virtual async Task<SessionUnit> GetAndCheckSessionUnitAsync(Guid sessionUnitId)
    {
        var sessionunit = await SessionUnitManager.GetAsync(sessionUnitId);

        return sessionunit;
    }

    /// <summary>
    /// 发[文本]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<TextContentInfo>> SendTextAsync(Guid sessionUnitId, MessageInput<TextContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendTextAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[命令]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<CmdContentInfo>> SendCmdAsync(Guid sessionUnitId, MessageInput<CmdContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendCmdAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[链接]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<LinkContentInfo>> SendLinkAsync(Guid sessionUnitId, MessageInput<LinkContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendLinkAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[HTML]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<HtmlContentInfo>> SendHtmlAsync(Guid sessionUnitId, MessageInput<HtmlContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendHtmlAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[图片]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<ImageContentInfo>> SendImageAsync(Guid sessionUnitId, MessageInput<ImageContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendImageAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[语音]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<SoundContentInfo>> SendSoundAsync(Guid sessionUnitId, MessageInput<SoundContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendSoundAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[视频]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<VideoContentInfo>> SendVideoAsync(Guid sessionUnitId, MessageInput<VideoContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendVideoAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[文件]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<FileContentInfo>> SendFileAsync(Guid sessionUnitId, MessageInput<FileContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendFileAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[位置]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<LocationContentInfo>> SendLocationAsync(Guid sessionUnitId, MessageInput<LocationContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendLocationAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[名片]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<ContactsContentInfo>> SendContactsAsync(Guid sessionUnitId, MessageInput<ContactsContentInfo> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendContactsAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[聊天记录]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<HistoryContentOutput>> SendHistoryAsync(Guid sessionUnitId, MessageInput<HistoryContentInput> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendHistoryAsync(sessionunit, input);
    }

    /// <summary>
    /// 发[红包]消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<MessageInfo<RedEnvelopeContentOutput>> SendRedEnvelopeAsync(Guid sessionUnitId, MessageInput<RedEnvelopeContentInput> input)
    {
        var sessionunit = await GetAndCheckSessionUnitAsync(sessionUnitId);

        return await MessageSender.SendRedEnvelopeAsync(sessionunit, input);
    }


    /// <summary>
    /// 转发消息
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="messageId"> 消息Id</param>
    /// <param name="targets">目标</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<List<MessageDto>> ForwardAsync(Guid sessionUnitId, long messageId, List<Guid> targets)
    {
        var messageList = await MessageManager.ForwardAsync(sessionUnitId, messageId, targets);

        return ObjectMapper.Map<List<Message>, List<MessageDto>>(messageList);
    }


    /// <summary>
    /// 撤回消息
    /// </summary>
    /// <param name="messageId"> 消息Id</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<Dictionary<string, long>> RollbackAsync(long messageId)
    {
        var message = await Repository.GetAsync(messageId);
        return await MessageManager.RollbackAsync(message);
    }
}
