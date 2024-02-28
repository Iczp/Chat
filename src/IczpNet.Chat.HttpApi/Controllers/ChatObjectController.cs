using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers
{
    [Area(ChatRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
    [Route($"/api/{ChatRemoteServiceConsts.ModuleName}/chat-object")]
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
        [Route("{id}/update-protrait")]
        public async Task<BlobDto> UpdatePortraitAsync(long id, IFormFile file)
        {
            var blob = await UploadFileAsync(file, UserPortraitsContainer, $"{id}", true);

            await ChatObjectAppService.UpdatePortraitAsync(id, $"/file?id={blob.Id}");

            return blob;
        }
    }
}
