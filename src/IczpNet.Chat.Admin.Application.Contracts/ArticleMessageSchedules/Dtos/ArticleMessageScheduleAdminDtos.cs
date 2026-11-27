using System;
using System.ComponentModel.DataAnnotations;
using IczpNet.Chat.MessageSections;
using IczpNet.Chat.MessageSections.Templates;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.ArticleMessageSchedules.Dtos;

public class ArticleMessageScheduleAdminCreateInput
{
    public Guid Id { get; set; }

    public Guid SenderSessionUnitId { get; set; }

    public DateTimeOffset ScheduledAt { get; set; }

    [Required]
    public MessageInput<ArticleContentInput> Message { get; set; }
}

public class ArticleMessageScheduleAdminTimeInput
{
    public DateTimeOffset ScheduledAt { get; set; }
}

public class ArticleMessageScheduleAdminGetListInput : PagedResultRequestDto
{
    public Guid? SenderSessionUnitId { get; set; }

    public long? OwnerId { get; set; }

    public ArticleScheduleStatus? Status { get; set; }
}

public class ArticleMessageScheduleAdminDto : EntityDto<Guid>
{
    public Guid SenderSessionUnitId { get; set; }

    public long OwnerId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public ArticleScheduleStatus Status { get; set; }

    public int Attempts { get; set; }

    public DateTime? SentAt { get; set; }

    public long? MessageId { get; set; }

    public string LastError { get; set; }

    public ArticleContentInput Content { get; set; }
}
