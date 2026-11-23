using System;
using System.Threading.Tasks;
using IczpNet.Chat.MessageTemplates.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace IczpNet.Chat.MessageTemplates;

public interface IMessageTemplateAppService : IApplicationService
{
    /// <summary>
    /// 获取模板分页列表
    /// </summary>
    Task<PagedResultDto<MessageTemplateDto>> GetListAsync(MessageTemplateGetListInput input);

    /// <summary>
    /// 获取模板草稿详情
    /// </summary>
    Task<MessageTemplateDetailDto> GetAsync(Guid id);

    /// <summary>
    /// 创建模板草稿
    /// </summary>
    Task<MessageTemplateDetailDto> CreateAsync(MessageTemplateCreateInput input);

    /// <summary>
    /// 修改模板草稿
    /// </summary>
    Task<MessageTemplateDetailDto> UpdateAsync(Guid id, MessageTemplateUpdateInput input);

    /// <summary>
    /// 发布模板新版本
    /// </summary>
    Task<MessageTemplatePublishOutput> PublishAsync(Guid id, MessageTemplatePublishInput input);

    /// <summary>
    /// 设置启用/停用状态
    /// </summary>
    Task<MessageTemplateDetailDto> SetEnabledAsync(Guid id, MessageTemplateSetEnabledInput input);

    /// <summary>
    /// 获取历史版本分页列表
    /// </summary>
    Task<PagedResultDto<MessageTemplateRevisionDto>> GetRevisionsAsync(Guid id, PagedResultRequestDto input);

    /// <summary>
    /// 获取指定版本详情
    /// </summary>
    Task<MessageTemplateRevisionDetailDto> GetRevisionAsync(Guid id, int version);

    /// <summary>
    /// 纯预览验证
    /// </summary>
    Task<MessageTemplatePreviewOutput> PreviewAsync(MessageTemplatePreviewInput input);
}
