using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.ChatObjectCategorys;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.ChatObjectTypes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.OfficialSections.Officials.Dtos;
using IczpNet.Chat.SessionSections.SessionUnits.Dtos;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.OfficialSections.Officials;

public class OfficialAppService : ChatAppService, IOfficialAppService
{
    public virtual string InvitePolicyName { get; set; }
    public virtual string CreateOfficialPolicyName { get; set; }
    protected IOfficialManager OfficialManager { get; }
    protected IChatObjectTypeManager ChatObjectTypeManager { get; }

    protected IChatObjectCategoryManager ChatObjectCategoryManager { get; }

    public OfficialAppService(IOfficialManager roomManager, 
        IChatObjectCategoryManager chatObjectCategoryManager, 
        IChatObjectTypeManager chatObjectTypeManager)
    {
        OfficialManager = roomManager;
        ChatObjectCategoryManager = chatObjectCategoryManager;
        ChatObjectTypeManager = chatObjectTypeManager;
    }


    [HttpPost]
    public virtual async Task<ChatObjectDto> CreateAsync(OfficialCreateInput input)
    {
        await CheckPolicyAsync(CreateOfficialPolicyName);

        var chatObjectType = await ChatObjectTypeManager.GetAsync(ChatObjectTypeEnums.ShopKeeper);

        var official = await OfficialManager.CreateAsync(new ChatObject(input.Name, chatObjectType, null));

        return ObjectMapper.Map<ChatObject, ChatObjectDto>(official);
    }

    [HttpPost]
    public Task<SessionUnitOwnerDto> EnableAsync(long ownerId, long destinationId)
    {
        throw new NotImplementedException();
    }

    [HttpPost]
    public Task<SessionUnitOwnerDto> DisableAsync(Guid sessionUnitId)
    {
        throw new NotImplementedException();
    }
}
