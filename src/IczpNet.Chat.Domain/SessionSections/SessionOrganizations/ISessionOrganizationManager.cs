using IczpNet.AbpTrees;

namespace IczpNet.Chat.SessionSections.SessionOrganizations;

public interface ISessionOrganizationManager : ITreeManager<SessionOrganization,long, SessionOrganizationInfo>
{
}
