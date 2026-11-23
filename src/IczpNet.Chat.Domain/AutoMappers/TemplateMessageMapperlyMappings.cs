using System;
using System.Text.Json;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainTemplateContentMapper : MapperBase<TemplateContent, TemplateContentInfo>
{
    public override TemplateContentInfo Map(TemplateContent source)
    {
        if (source is null) return default!;
        var result = new TemplateContentInfo();
        Map(source, result);
        return result;
    }

    public override void Map(TemplateContent source, TemplateContentInfo destination)
    {
        if (source is null || destination is null) return;

        if (!string.IsNullOrWhiteSpace(source.SnapshotJson))
        {
            try
            {
                var snapshot = JsonSerializer.Deserialize<TemplateContentInfo>(source.SnapshotJson);
                if (snapshot != null)
                {
                    destination.SchemaVersion = snapshot.SchemaVersion;
                    destination.TemplateId = snapshot.TemplateId;
                    destination.TemplateVersion = snapshot.TemplateVersion;
                    destination.TemplateName = snapshot.TemplateName;
                    destination.FallbackText = snapshot.FallbackText;
                    destination.Data = snapshot.Data;
                    destination.Layout = snapshot.Layout;
                    destination.Actions = snapshot.Actions;
                    destination.Resources = snapshot.Resources;
                }
            }
            catch
            {
                // Fallback to scalar properties if snapshot parse fails
            }
        }

        destination.Id = source.Id;
        destination.FallbackText ??= source.FallbackText;
        if (source.TemplateId != Guid.Empty)
        {
            destination.TemplateId = source.TemplateId;
        }
        if (source.TemplateVersion > 0)
        {
            destination.TemplateVersion = source.TemplateVersion;
        }
        destination.SchemaVersion = source.SchemaVersion > 0 ? source.SchemaVersion : 1;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainMessageToTemplateMessageMapper(ChatDomainSessionUnitToSessionUnitSenderInfoMapper senderMapper)
    : MapperBase<Message, MessageInfo<TemplateContentInfo>>
{
    [UseMapper]
    private readonly ChatDomainSessionUnitToSessionUnitSenderInfoMapper _senderMapper = senderMapper;

    public override MessageInfo<TemplateContentInfo> Map(Message source)
    {
        if (source is null) return default!;
        var result = new MessageInfo<TemplateContentInfo>();
        Map(source, result);
        return result;
    }

    public override void Map(Message source, MessageInfo<TemplateContentInfo> destination)
    {
        MapFields(source, destination);
        destination.Content = (TemplateContentInfo)source.Content;
    }

    [MapperIgnoreTarget(nameof(MessageInfo<TemplateContentInfo>.Content))]
    private partial void MapFields(Message source, MessageInfo<TemplateContentInfo> destination);
}
