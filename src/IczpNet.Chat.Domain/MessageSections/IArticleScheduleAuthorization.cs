using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IczpNet.Chat.MessageSections;

// Identity integration lives in Application; Domain does not depend on its implementation.
public interface IArticleScheduleAuthorization
{
    Task<ClaimsPrincipal> AuthorizeAsync(Guid userId, long ownerId);
}
