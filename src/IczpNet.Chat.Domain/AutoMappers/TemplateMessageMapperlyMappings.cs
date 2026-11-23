using System;
using System.Text.Json;
using IczpNet.Chat.MessageSections.Messages;
using IczpNet.Chat.MessageSections.Templates;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using Microsoft.Extensions.Logging;
using IczpNet.Chat.MessageTemplates;
using System.Text;

namespace IczpNet.Chat.AutoMappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainTemplateContentMapper(ILogger<ChatDomainTemplateContentMapper> logger = null) : MapperBase<TemplateContent, TemplateContentInfo>
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
                var snapshot = Encoding.UTF8.GetByteCount(source.SnapshotJson) <= MessageTemplateConsts.MaxSnapshotBytes
                    ? JsonSerializer.Deserialize<TemplateContentInfo>(source.SnapshotJson) : null;
                if (snapshot != null && snapshot.TemplateId == source.TemplateId &&
                    snapshot.TemplateVersion == source.TemplateVersion && snapshot.SchemaVersion == source.SchemaVersion &&
                    snapshot.FallbackText == source.FallbackText)
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
                else
                    logger?.LogWarning("模板快照元数据不一致，已降级摘要。TemplateId={TemplateId}, Version={Version}", source.TemplateId, source.TemplateVersion);
            }
            catch (JsonException)
            {
                logger?.LogWarning("模板快照JSON损坏，已降级摘要。TemplateId={TemplateId}, Version={Version}", source.TemplateId, source.TemplateVersion);
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
