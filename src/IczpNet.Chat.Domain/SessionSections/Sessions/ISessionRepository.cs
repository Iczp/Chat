using System;
using Volo.Abp.Domain.Repositories;

namespace IczpNet.Chat.SessionSections.Sessions
{
    public interface ISessionRepository : IRepository<Session, Guid>
    {

    }
}
