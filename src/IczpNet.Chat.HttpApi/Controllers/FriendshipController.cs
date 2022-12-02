using IczpNet.Chat.SessionSections.Friendships;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Controllers
{
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
