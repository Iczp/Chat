using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.SessionUnits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Controllers
{
    [Route($"/api/{ChatRemoteServiceConsts.ModuleName}/message-sender")]
    public class MessageSenderController : ChatController
    {
        protected IMessageSenderAppService MessageSenderAppService { get; set; }
        protected ISessionUnitManager SessionUnitManager { get; set; }


        public MessageSenderController(
            ISessionUnitManager sessionUnitManager,
            IMessageSenderAppService messageSenderAppService)
        {
            SessionUnitManager = sessionUnitManager;
            MessageSenderAppService = messageSenderAppService;
        }

        /// <summary>
        /// 上传并发送(file/sound/image/video)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="quoteMessageId">引消息Id</param>
        /// <param name="remindList">提醒（@XXX）</param>
        /// <param name="sessionUnitId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("send-upload/{sessionUnitId}")]
        public async Task<IActionResult> UploadToSendAsync(Guid sessionUnitId, IFormFile file, long quoteMessageId, List<Guid> remindList)
        {
            var sessionUnit = await SessionUnitManager.GetAsync(sessionUnitId);

            var blob = await UploadFileAsync(file, ChatFilesContainer, $"{sessionUnit.SessionId}/{sessionUnitId}", false);

            var sendResult = await MessageSenderAppService.SendFileAsync(sessionUnitId, new MessageInput<FileContentInfo>()
            {
                QuoteMessageId = quoteMessageId,
                RemindList = remindList,
                Content = new FileContentInfo()
                {
                    ContentType = file.ContentType,
                    Size = blob.FileSize,
                    FileName = file.FileName,
                    Suffix = blob.Suffix,
                    Url = $"/file?id={blob.Id}",
                }
            });
            return new JsonResult(sendResult);
        }
    }
}
