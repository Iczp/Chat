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
    [Route($"Api/{ChatRemoteServiceConsts.ModuleName}[Controller]/[Action]")]
    public class FriendshipController : ChatController
    {
        protected IFriendshipAppService FriendshipAppService { get; }

        public FriendshipController(IFriendshipAppService friendshipAppService)
        {
            FriendshipAppService = friendshipAppService;
        }

        [HttpPost]
        public Task<string> SetBackgroundImageAsync(Guid friendshipId, IFormFile backgroundImage)
        {
            return FriendshipAppService.SetBackgroundImageAsync(friendshipId, "");
        }
    }
}
