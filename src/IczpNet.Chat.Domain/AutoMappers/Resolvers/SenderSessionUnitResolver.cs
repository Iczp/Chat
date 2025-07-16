using AutoMapper;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.SessionSections.SessionUnits;
using IczpNet.Chat.SessionUnits;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;
using Volo.Abp.ObjectMapping;

namespace IczpNet.Chat.AutoMappers.Resolvers;

public class SenderSessionUnitResolver<TDestination>(
    ISessionUnitManager sessionUnitManager,
    IObjectMapper objectMapper
    ) : DomainService, IValueResolver<Message, TDestination, SessionUnitSenderInfo>, ITransientDependency
{
    public ISessionUnitManager SessionUnitManager { get; } = sessionUnitManager;
    public IObjectMapper ObjectMapper { get; } = objectMapper;

    public SessionUnitSenderInfo Resolve(Message source, TDestination destination, SessionUnitSenderInfo destMember, ResolutionContext context)
    {
        if (!source.SenderSessionUnitId.HasValue)
        {
            return null;
        }

        var senderSessionUnit = SessionUnitManager.GetAsync(source.SenderSessionUnitId.Value).GetAwaiter().GetResult();

        return ObjectMapper.Map<SessionUnit, SessionUnitSenderInfo>(senderSessionUnit);
    }
}
