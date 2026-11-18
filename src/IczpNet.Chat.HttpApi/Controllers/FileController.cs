using IczpNet.Chat.Blobs;
using IczpNet.Chat.Blobs.Dtos;
using IczpNet.Chat.ChatObjects;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.Options;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Blob = IczpNet.Chat.Blobs.Blob;

namespace IczpNet.Chat.Controllers;

[Route("[Controller]")]
public class FileController(
    IBlobAccessAuthorizationService blobAccessAuthorizationService,
    IBlobSignatureService blobSignatureService,
    IBlobDirectUrlProvider blobDirectUrlProvider,
    IChatObjectManager chatObjectManager,
    ISessionUnitManager sessionUnitManager,
    IMessageRepository messageRepository,
    IOptions<AttachmentSignatureOptions> signatureOptions) : ChatController
{
    [HttpGet]
    [Route("{id}")]
    [Route("")]
    [Route("/api/chat/file/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAsync(
        Guid id,
        [FromQuery] Guid? su = null,
        [FromQuery] long? msg = null,
        [FromQuery] long? rootMsg = null,
        [FromQuery] long? exp = null,
        [FromQuery] string sig = null)
    {
        bool hasValidSignature = false;
        if (exp.HasValue && !string.IsNullOrWhiteSpace(sig) && su.HasValue)
        {
            hasValidSignature = blobSignatureService.ValidateSignature(id, su.Value, msg, rootMsg, exp.Value, sig);
            if (!hasValidSignature)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "Invalid or expired signature." });
            }
        }

        IReadOnlyCollection<long> ownerIds = Array.Empty<long>();
        if (CurrentUser.Id.HasValue)
        {
            ownerIds = await chatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        }

        Blob blob;
        try
        {
            blob = await blobAccessAuthorizationService.CheckAccessAsync(
                id,
                su,
                msg,
                rootMsg,
                ownerIds,
                hasValidSignature,
                HttpContext.RequestAborted);
        }
        catch (BusinessException ex) when (ex.Code == ChatErrorCodes.Blobs.AccessDenied)
        {
            return CurrentUser.Id.HasValue || hasValidSignature
                ? StatusCode(StatusCodes.Status403Forbidden, new { message = "Access denied." })
                : Unauthorized(new { message = "Authentication or valid signature required." });
        }

        if (blob == null)
        {
            return NotFound(new { message = "blob is null" });
        }

        var expiry = TimeSpan.FromSeconds(Math.Max(signatureOptions.Value.DefaultExpireSeconds, 60));
        var directUrl = await blobDirectUrlProvider.TryGetPresignedUrlAsync(blob, expiry, HttpContext.RequestAborted);
        if (!string.IsNullOrWhiteSpace(directUrl))
        {
            return Redirect(directUrl);
        }

        try
        {
            var bytes = await BlobManager.GetBytesAsync(blob.Container, blob.Name);
            return File(bytes, blob.MimeType, blob.FileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Blob.ID={blob.Id},Container={blob.Container},Name={blob.Name},MimeType={blob.MimeType},FileName={blob.FileName}");
            return NotFound();
        }
    }

    /// <summary>
    /// 换取短时效签名访问地址（用于前端媒体渲染或直出下载）
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("/api/chat/file/sign")]
    public async Task<IActionResult> SignAsync(
        [FromQuery] Guid blobId,
        [FromQuery] Guid sessionUnitId,
        [FromQuery] long? messageId = null,
        [FromQuery] long? rootMessageId = null)
    {
        if (!CurrentUser.Id.HasValue)
        {
            throw new AbpAuthorizationException("Authentication required.");
        }

        var unit = await sessionUnitManager.GetCacheAsync(sessionUnitId);
        var ownerIds = await chatObjectManager.GetIdListByUserIdAsync(CurrentUser.Id.Value);
        if (!ownerIds.Contains(unit.OwnerId))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var targetMsgId = rootMessageId ?? messageId;
        if (targetMsgId.HasValue && !await messageRepository.CanReadAsync(sessionUnitId, targetMsgId.Value))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var baseUrl = await BlobResolver.GetFileUrlAsync(blobId, string.Empty);
        var signedUrl = blobSignatureService.AppendSignature(baseUrl, blobId, sessionUnitId, messageId, rootMessageId);
        return Ok(new { url = signedUrl });
    }

    [HttpPost]
    [Authorize]
    public async Task<BlobDto> UploadAsync(IFormFile file, string container, string folder, bool isPublic)
    {
        var entity = await UploadFileAsync(GuidGenerator.Create(), file, container, folder, isPublic);

        return ObjectMapper.Map<Blob, BlobDto>(entity);
    }
}
