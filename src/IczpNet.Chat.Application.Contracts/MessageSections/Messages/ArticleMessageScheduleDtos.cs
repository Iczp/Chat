using IczpNet.Chat.MessageSections.Templates;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections.Messages;

public class ArticleMessageScheduleCreateInput
{
    public Guid Id { get; set; }
    public Guid SenderSessionUnitId { get; set; }
    public DateTimeOffset ScheduledAt { get; set; }
    [Required] public MessageInput<ArticleContentInput> Message { get; set; }
}
public class ArticleMessageScheduleTimeInput
{
    public DateTimeOffset ScheduledAt { get; set; }
}
public class ArticleMessageScheduleGetListInput : PagedResultRequestDto
{
    public Guid SenderSessionUnitId { get; set; }
}
public class ArticleMessageScheduleDto : EntityDto<Guid>
{
    public Guid SenderSessionUnitId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public ArticleScheduleStatus Status { get; set; }
    public int Attempts { get; set; }
    public DateTime? SentAt { get; set; }
    public long? MessageId { get; set; }
    public string LastError { get; set; }
    public ArticleContentInput Content { get; set; }
}
public interface IArticleMessageScheduleAppService : IApplicationService
{
    Task<ArticleMessageScheduleDto> CreateAsync(ArticleMessageScheduleCreateInput input);
    Task<PagedResultDto<ArticleMessageScheduleDto>> GetListAsync(ArticleMessageScheduleGetListInput input);
    Task<ArticleMessageScheduleDto> RescheduleAsync(Guid id, ArticleMessageScheduleTimeInput input);
    Task<ArticleMessageScheduleDto> CancelAsync(Guid id);
    Task<ArticleMessageScheduleDto> ExecuteNowAsync(Guid id);
}
