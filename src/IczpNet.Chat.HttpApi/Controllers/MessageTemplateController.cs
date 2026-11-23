using System;
using System.Threading.Tasks;
using IczpNet.Chat.MessageTemplates;
using IczpNet.Chat.MessageTemplates.Dtos;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Controllers;

[Area(ChatRemoteServiceConsts.ModuleName)]
[RemoteService(Name = ChatRemoteServiceConsts.RemoteServiceName)]
[Route($"/api/{ChatRemoteServiceConsts.ModuleName}/message-template")]
public class MessageTemplateController(IMessageTemplateAppService templateAppService)
    : ChatController, IMessageTemplateAppService
{
    protected IMessageTemplateAppService TemplateAppService { get; } = templateAppService;

    [HttpGet]
    public virtual Task<PagedResultDto<MessageTemplateDto>> GetListAsync([FromQuery] MessageTemplateGetListInput input)
    {
        return TemplateAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<MessageTemplateDetailDto> GetAsync(Guid id)
    {
        return TemplateAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<MessageTemplateDetailDto> CreateAsync([FromBody] MessageTemplateCreateInput input)
    {
        return TemplateAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<MessageTemplateDetailDto> UpdateAsync(Guid id, [FromBody] MessageTemplateUpdateInput input)
    {
        return TemplateAppService.UpdateAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/publish")]
    public virtual Task<MessageTemplatePublishOutput> PublishAsync(Guid id, [FromBody] MessageTemplatePublishInput input)
    {
        return TemplateAppService.PublishAsync(id, input);
    }

    [HttpPost]
    [Route("{id}/set-enabled")]
    public virtual Task<MessageTemplateDetailDto> SetEnabledAsync(Guid id, [FromBody] MessageTemplateSetEnabledInput input)
    {
        return TemplateAppService.SetEnabledAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/revisions")]
    public virtual Task<PagedResultDto<MessageTemplateRevisionDto>> GetRevisionsAsync(Guid id, [FromQuery] PagedResultRequestDto input)
    {
        return TemplateAppService.GetRevisionsAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/revisions/{version}")]
    public virtual Task<MessageTemplateRevisionDetailDto> GetRevisionAsync(Guid id, int version)
    {
        return TemplateAppService.GetRevisionAsync(id, version);
    }

    [HttpPost]
    [Route("preview")]
    public virtual Task<MessageTemplatePreviewOutput> PreviewAsync([FromBody] MessageTemplatePreviewInput input)
    {
        return TemplateAppService.PreviewAsync(input);
    }
}
