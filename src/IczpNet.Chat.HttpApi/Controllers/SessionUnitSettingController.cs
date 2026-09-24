using IczpNet.Chat.SessionUnits.Dtos;
using IczpNet.Chat.SessionUnitSettings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/session-unit-setting")]
public class SessionUnitSettingController(
    ISessionUnitSettingAppService sessionUnitSettingAppService) : ChatController
{
    protected ISessionUnitSettingAppService SessionUnitSettingAppService { get; } = sessionUnitSettingAppService;

    /// <summary>
    /// 上传并设置聊天背景图片
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <param name="file">背景图片</param>
    /// <returns>更新后的会话单元</returns>
    [HttpPost("set-background-image/{sessionUnitId}")]
    [Consumes("multipart/form-data")]
    public async Task<SessionUnitOwnerDto> SetBackgroundImageAsync(Guid sessionUnitId, IFormFile file)
    {
        await SessionUnitSettingAppService.CheckSetBackgroundImageAsync(sessionUnitId);
        await CheckImageAsync(file);

        var blobId = GuidGenerator.Create();
        var folder = $"{DateDirectoryName}/background-images/{sessionUnitId}";
        var blob = await UploadFileAsync(blobId, file, ChatFilesContainer, folder, true);
        var backgroundImageUrl = await GetFileUrlAsync(blob.Id, file.ContentType);

        return await SessionUnitSettingAppService.SetBackgroundImageAsync(sessionUnitId, backgroundImageUrl);
    }

    /// <summary>
    /// 清除聊天背景图片
    /// </summary>
    /// <param name="sessionUnitId">会话单元Id</param>
    /// <returns>更新后的会话单元</returns>
    [HttpPost("clear-background-image/{sessionUnitId}")]
    public Task<SessionUnitOwnerDto> ClearBackgroundImageAsync(Guid sessionUnitId)
    {
        return SessionUnitSettingAppService.ClearBackgroundImageAsync(sessionUnitId);
    }
}
