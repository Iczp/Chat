using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.SessionSections.Friendships;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers
{

    [Area(ChatRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
    //[Route($"Api/{ChatRemoteServiceConsts.ModuleName}[Controller]/[Action]")]
    public class ChatObjectController : ChatController
    {
        protected IChatObjectAppService ChatObjectAppService { get; }

        public ChatObjectController(IChatObjectAppService chatObjectAppService)
        {
            ChatObjectAppService = chatObjectAppService;
        }

        [HttpPost]
        public Task<ChatObjectDto> UpdatePortraitAsync(long id, IFormFile portrait)
        {
            return ChatObjectAppService.UpdatePortraitAsync(id, "");
        }
    }
}
