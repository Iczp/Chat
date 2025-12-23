using AutoMapper;
using Volo.Abp.Auditing;
using Volo.Abp.AutoMapper;
using Volo.Abp.Domain.Entities;

namespace IczpNet.Chat.AutoMappers;

public static class AutoMapperExtensions
{

    public static IMappingExpression<TSource, TDestination> UsingMessageTemplate<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression)
        where TDestination : IFullAuditedObject, IHasConcurrencyStamp//, IEntity<Guid>, IMultiTenant

    {
        return mappingExpression
            .IgnoreAllPropertiesWithAnInaccessibleSetter()
            .IgnoreAllSourcePropertiesWithAnInaccessibleSetter()
            .IgnoreFullAuditedObjectProperties()
            //.Ignore(x => x.Id)
            //.Ignore(x => x.TenantId)
            .Ignore(x => x.ConcurrencyStamp);
    }

    //public static IMappingExpression<Message, TDestination> UsingMessageOutput<TSource, TDestination>(this IMappingExpression<TSource, TDestination> mappingExpression)
    //    where TDestination : IMessageOuput
    //{
    //    return mappingExpression.ForMember(x => x.Content, o => o.MapFrom(x => x.GetMessageContent().ToDynamicList().FirstOrDefault()));
    //}
}
