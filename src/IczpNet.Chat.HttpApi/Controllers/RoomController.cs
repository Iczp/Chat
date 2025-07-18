﻿using IczpNet.Chat.ChatObjects.Dtos;
using IczpNet.Chat.RoomSections.Rooms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/room")]
public class RoomController(IRoomAppService roomAppService) : ChatController
{
    protected IRoomAppService RoomAppService { get; } = roomAppService;

    /// <summary>
    /// 上传群头像
    /// </summary>
    /// <param name="sessionUnitId">主建Id</param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("upload-portrait/{sessionUnitId}")]
    public async Task<ChatObjectDto> UploadPortraitAsync(Guid sessionUnitId, IFormFile file)
    {
        //Assert.If(file == null, "No file found!");

        //var blobId = GuidGenerator.Create();

        //var chatObjectDto = await RoomAppService.UpdatePortraitWithTaskAsync(sessionUnitId, $"/file?id={blobId}", $"/file?id={blobId}");

        //var blob = await UploadFileAsync(blobId, file, PortraitsContainer, $"{chatObjectDto.Id}", true);

        await CheckImageAsync(file);

        var bigImgBlobId = GuidGenerator.Create();

        var thumbnailBlobId = GuidGenerator.Create();

        var thumbnailUrl = await GetFileUrlAsync(thumbnailBlobId);

        var portraitUrl = await GetFileUrlAsync(bigImgBlobId);

        var chatObjectDto = await RoomAppService.UpdatePortraitAsync(sessionUnitId, thumbnailUrl, portraitUrl);

        await SavePortraitAsync(file, chatObjectDto.Id, thumbnailBlobId, bigImgBlobId);

        return chatObjectDto;
    }
}
