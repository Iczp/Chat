﻿using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionRepository : IRepository<Session, Guid>
    {
        Task<int> UpdateLastMessageIdAsync(Guid sessionId, long lastMessageId);
    }
}
