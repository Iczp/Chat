using IczpNet.AbpCommons;
using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.DataFilters;
using IczpNet.Chat.RoomSections.Rooms;
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
        /// 上传头像
        /// </summary>
        /// <param name="id">主建Id</param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}/upload-portrait")]
        public async Task<BlobDto> UploadPortraitAsync(long id, IFormFile file)
        {
            Assert.If(file == null, "No file found!");

            var blobId = GuidGenerator.Create();

            await ChatObjectAppService.UpdatePortraitAsync(id, $"/file?id={blobId}");

            return await UploadFileAsync(blobId, file, PortraitsContainer, $"{id}", true);
        }
    }
}
