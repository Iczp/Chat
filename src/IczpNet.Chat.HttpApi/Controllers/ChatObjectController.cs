using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
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

        /// <summary>
        /// 更新头像
        /// </summary>
        /// <param name="id">主建Id</param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> UpdatePortraitAsync(long id, IFormFile file)
        {
            await Task.Yield();
            return $"id:{id}/{file.FileName}";
            //return null;
            //return ChatObjectAppService.UpdatePortraitAsync(id, "");
        }
    }
}
