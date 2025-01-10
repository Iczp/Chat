using IczpNet.Chat.ChatObjects;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.Robots;

public class RobotManager : DomainService, IRobotManager
{
    protected IChatObjectRepository Repository { get; }
    protected IChatObjectManager ChatObjectManager { get; }

    public RobotManager(IChatObjectRepository repository, IChatObjectManager chatObjectManager)
    {
        Repository = repository;
        ChatObjectManager = chatObjectManager;
    }

    
}
