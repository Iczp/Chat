using AutoMapper;
using Volo.Abp.Domain.Services;

namespace IczpNet.Chat.UrlNormalizers;

public class UrlResolver : DomainService, IMemberValueResolver<object, object, string, string>
{
    protected IUrlNormalizer UrlNormalizer => LazyServiceProvider.LazyGetRequiredService<IUrlNormalizer>();

    public string Resolve(object source, object destination, string sourceMember, string destMember, ResolutionContext context)
    {
        return UrlNormalizer.Normalize(sourceMember);
    }
}