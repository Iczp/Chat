using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.OfficialSections.Officials.Dtos;
using IczpNet.Chat.SessionUnits;
using IczpNet.Chat.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.OfficialSections.Officials;

/// <summary>
/// 公众号
/// </summary>
public class OfficialAppService : ChatAppService, IOfficialAppService
{
    public virtual string InvitePolicyName { get; set; }
    public virtual string CreateOfficialPolicyName { get; set; }
    protected IOfficialManager OfficialManager { get; }
    protected IChatObjectTypeManager ChatObjectTypeManager { get; }
    //protected ISessionUnitManager SessionUnitManager { get; }
    protected IMessageSender MessageSender { get; }
    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }

    public OfficialAppService(IOfficialManager roomManager,
        IChatObjectCategoryManager chatObjectCategoryManager,
        IChatObjectTypeManager chatObjectTypeManager,
        IMessageSender messageSender)
    {
        OfficialManager = roomManager;
        ChatObjectCategoryManager = chatObjectCategoryManager;
        ChatObjectTypeManager = chatObjectTypeManager;
        MessageSender = messageSender;
    }

    /// <summary>
    /// 创建公众号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateAsync(OfficialCreateInput input)
    {
        await CheckPolicyAsync(CreateOfficialPolicyName);

        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.Official);

        var chatObject = new ChatObject(input.Name, chatObjectType, null)
        {
            Code = input.Code,
            //Portrait = input.Portrait,
            Description = input.Description,
        };

        chatObject.SetPortrait(input.Thumbnail, input.Portrait);

        var official = await ChatObjectManager.CreateAsync(chatObject, true);

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(official);
    }

    /// <summary>
    /// 关注公众号[ownerId]
    /// </summary>
    /// <param name="ownerId">聊天对象Id</param>
    /// <param name="destinationId"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SubscribeAsync(long ownerId, long destinationId)
    {
        var sessionUnit = await OfficialManager.SubscribeAsync(ownerId, destinationId);

        return ObjectMapper.Map<SessionUnit, SessionUnitOwnerDto>(sessionUnit);
    }

    /// <summary>
    /// 取消关注公众号
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> UnsubscribeAsync(Guid sessionUnitId)
    {
        var sessionUnit = await OfficialManager.UnsubscribeAsync(sessionUnitId);

        return ObjectMapper.Map<SessionUnit, SessionUnitOwnerDto>(sessionUnit);
    }

    /// <summary>
    /// 关注公众号[sessionUnitId]
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<SessionUnitOwnerDto> SubscribeByIdAsync(Guid sessionUnitId)
    {
        var sessionUnit = await OfficialManager.SubscribeByIdAsync(sessionUnitId);

        return ObjectMapper.Map<SessionUnit, SessionUnitOwnerDto>(sessionUnit);
    }


}
