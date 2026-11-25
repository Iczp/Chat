using IczpNet.Chat.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Announcements.Dtos;

public class SessionAnnouncementRevisionDto : EntityDto<Guid>
{
    public virtual Guid AnnouncementId { get; set; }
    public virtual int Version { get; set; }
    public virtual string Title { get; set; }
    public virtual string Content { get; set; }
    public virtual EditorTypes EditorType { get; set; }
    public virtual Guid EditorSessionUnitId { get; set; }
    public virtual long EditorChatObjectId { get; set; }
    public virtual string EditorName { get; set; }
    public virtual string EditorPortrait { get; set; }
    public virtual string ChangeSummary { get; set; }
    public virtual DateTime CreationTime { get; set; }
}
