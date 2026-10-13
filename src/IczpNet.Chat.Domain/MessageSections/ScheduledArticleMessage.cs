using IczpNet.Chat.BaseEntities;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;

namespace IczpNet.Chat.MessageSections;

public class ScheduledArticleMessage : BaseEntity<Guid>
{
    public Guid SenderSessionUnitId { get; private set; }
    public long OwnerId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public Guid? RequestedByTenantId { get; private set; }
    [Required] public string InputJson { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public ArticleScheduleStatus Status { get; private set; }
    public int Version { get; private set; }
    public int Attempts { get; private set; }
    public DateTime? LeaseUntil { get; private set; }
    public long? MessageId { get; private set; }
    public DateTime? SentAt { get; private set; }
    [StringLength(512)] public string LastError { get; private set; }

    protected ScheduledArticleMessage() { }
    public ScheduledArticleMessage(Guid id, Guid senderSessionUnitId, long ownerId, Guid requestedBy, string inputJson, DateTime scheduledAt, Guid? requestedByTenantId = null) : base(id)
    {
        SenderSessionUnitId = senderSessionUnitId;
        OwnerId = ownerId;
        RequestedBy = requestedBy;
        RequestedByTenantId = requestedByTenantId;
        InputJson = inputJson;
        ScheduledAt = scheduledAt;
        Version = 1;
    }

    public void Reschedule(DateTime time)
    {
        EnsureEditable();
        ScheduledAt = time;
        Version++;
        Attempts = 0;
        LeaseUntil = null;
        LastError = null;
        Status = ArticleScheduleStatus.Pending;
    }
    public void Cancel()
    {
        EnsureEditable();
        Status = ArticleScheduleStatus.Cancelled;
        Version++;
        LeaseUntil = null;
    }
    private void EnsureEditable()
    {
        if (Status != ArticleScheduleStatus.Pending && Status != ArticleScheduleStatus.Failed)
            throw new UserFriendlyException("只有待发送或失败的任务可以取消、改时或重新执行。");
    }
    public void MarkDispatched(DateTime leaseUntil) => LeaseUntil = leaseUntil;
    public void Start(DateTime leaseUntil)
    {
        Status = ArticleScheduleStatus.Running;
        Attempts++;
        LeaseUntil = leaseUntil;
    }
    public void Complete(long messageId, DateTime time)
    {
        MessageId = messageId;
        SentAt = time;
        Status = ArticleScheduleStatus.Sent;
        LeaseUntil = null;
        LastError = null;
    }
    public void Fail(string error)
    {
        Status = ArticleScheduleStatus.Failed;
        LeaseUntil = null;
        LastError = error?.Length > 512 ? error[..512] : error;
    }
}
