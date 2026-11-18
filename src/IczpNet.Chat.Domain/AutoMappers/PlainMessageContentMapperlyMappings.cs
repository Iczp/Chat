using IczpNet.Chat.MessageSections.Templates;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// These message templates have no URL resolver or other custom member rule.
// Entity identity and audit members are not writable from the input DTOs.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainHtmlContentInfoToHtmlContentMapper
    : TwoWayMapperBase<HtmlContentInfo, HtmlContent>
{
    public override partial HtmlContent Map(HtmlContentInfo source);
    public override partial void Map(HtmlContentInfo source, HtmlContent destination);
    public override partial HtmlContentInfo ReverseMap(HtmlContent source);
    public override partial void ReverseMap(HtmlContent source, HtmlContentInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainContactsContentInfoToContactsContentMapper
    : TwoWayMapperBase<ContactsContentInfo, ContactsContent>
{
    public override partial ContactsContent Map(ContactsContentInfo source);
    public override partial void Map(ContactsContentInfo source, ContactsContent destination);
    public override partial ContactsContentInfo ReverseMap(ContactsContent source);
    public override partial void ReverseMap(ContactsContent source, ContactsContentInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainLinkContentInfoToLinkContentMapper
    : TwoWayMapperBase<LinkContentInfo, LinkContent>
{
    public override partial LinkContent Map(LinkContentInfo source);
    public override partial void Map(LinkContentInfo source, LinkContent destination);
    public override partial LinkContentInfo ReverseMap(LinkContent source);
    public override partial void ReverseMap(LinkContent source, LinkContentInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainLocationContentInfoToLocationContentMapper
    : TwoWayMapperBase<LocationContentInfo, LocationContent>
{
    public override partial LocationContent Map(LocationContentInfo source);
    public override partial void Map(LocationContentInfo source, LocationContent destination);
    public override partial LocationContentInfo ReverseMap(LocationContent source);
    public override partial void ReverseMap(LocationContent source, LocationContentInfo destination);
}
