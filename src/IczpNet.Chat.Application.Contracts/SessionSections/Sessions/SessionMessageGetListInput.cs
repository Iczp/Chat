﻿using System;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public class SessionMessageGetListInput : PagedAndSortedResultRequestDto
    {
        public virtual Guid OwnerId { get; set; }

        public virtual Guid? SessionId { get; set; }

        public virtual Guid? SenderId { get; set; }

        public virtual bool IsUnreaded { get; set; }
    }
}