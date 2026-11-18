using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.UrlNormalizers;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace IczpNet.Chat.AutoMappers;

// Input URLs are stored unchanged. Output URLs use the same IUrlNormalizer as
// the former AutoMapper UrlResolver. Map-to-existing preserves entity audit
// state because Mapperly cannot write the protected audit/identity setters.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainFileContentInfoToFileContentMapper(IUrlNormalizer urlNormalizer)
    : TwoWayMapperBase<FileContentInfo, FileContent>
{
    public override partial FileContent Map(FileContentInfo source);
    public override partial void Map(FileContentInfo source, FileContent destination);

    public override FileContentInfo ReverseMap(FileContent source)
    {
        if (source is null) return default!;
        var destination = new FileContentInfo();
        ReverseMap(source, destination);
        return destination;
    }

    public override void ReverseMap(FileContent source, FileContentInfo destination)
    {
        if (source is null) return;
        MapOutputFields(source, destination);
        destination.Url = urlNormalizer.Normalize(source.Url);
        destination.ActionUrl = urlNormalizer.Normalize(source.ActionUrl);
    }

    [MapperIgnoreTarget(nameof(FileContentInfo.Url))]
    [MapperIgnoreTarget(nameof(FileContentInfo.ActionUrl))]
    private partial void MapOutputFields(FileContent source, FileContentInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainImageContentInfoToImageContentMapper(IUrlNormalizer urlNormalizer)
    : TwoWayMapperBase<ImageContentInfo, ImageContent>
{
    public override partial ImageContent Map(ImageContentInfo source);
    public override partial void Map(ImageContentInfo source, ImageContent destination);

    public override ImageContentInfo ReverseMap(ImageContent source)
    {
        if (source is null) return default!;
        var destination = new ImageContentInfo();
        ReverseMap(source, destination);
        return destination;
    }

    public override void ReverseMap(ImageContent source, ImageContentInfo destination)
    {
        if (source is null) return;
        MapOutputFields(source, destination);
        destination.Url = urlNormalizer.Normalize(source.Url);
        destination.ActionUrl = urlNormalizer.Normalize(source.ActionUrl);
        destination.ThumbnailUrl = urlNormalizer.Normalize(source.ThumbnailUrl);
        destination.ThumbnailActionUrl = urlNormalizer.Normalize(source.ThumbnailActionUrl);
    }

    [MapperIgnoreTarget(nameof(ImageContentInfo.Url))]
    [MapperIgnoreTarget(nameof(ImageContentInfo.ActionUrl))]
    [MapperIgnoreTarget(nameof(ImageContentInfo.ThumbnailUrl))]
    [MapperIgnoreTarget(nameof(ImageContentInfo.ThumbnailActionUrl))]
    private partial void MapOutputFields(ImageContent source, ImageContentInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainSoundContentInfoToSoundContentMapper(IUrlNormalizer urlNormalizer)
    : TwoWayMapperBase<SoundContentInfo, SoundContent>
{
    public override partial SoundContent Map(SoundContentInfo source);
    public override partial void Map(SoundContentInfo source, SoundContent destination);

    public override SoundContentInfo ReverseMap(SoundContent source)
    {
        if (source is null) return default!;
        var destination = new SoundContentInfo();
        ReverseMap(source, destination);
        return destination;
    }

    public override void ReverseMap(SoundContent source, SoundContentInfo destination)
    {
        if (source is null) return;
        MapOutputFields(source, destination);
        destination.Url = urlNormalizer.Normalize(source.Url);
    }

    [MapperIgnoreTarget(nameof(SoundContentInfo.Url))]
    private partial void MapOutputFields(SoundContent source, SoundContentInfo destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class ChatDomainVideoContentInfoToVideoContentMapper(IUrlNormalizer urlNormalizer)
    : TwoWayMapperBase<VideoContentInfo, VideoContent>
{
    public override partial VideoContent Map(VideoContentInfo source);
    public override partial void Map(VideoContentInfo source, VideoContent destination);

    public override VideoContentInfo ReverseMap(VideoContent source)
    {
        if (source is null) return default!;
        var destination = new VideoContentInfo();
        ReverseMap(source, destination);
        return destination;
    }

    public override void ReverseMap(VideoContent source, VideoContentInfo destination)
    {
        if (source is null) return;
        MapOutputFields(source, destination);
        destination.Url = urlNormalizer.Normalize(source.Url);
        destination.SnapshotUrl = urlNormalizer.Normalize(source.SnapshotUrl);
        destination.SnapshotThumbnailUrl = urlNormalizer.Normalize(source.SnapshotThumbnailUrl);
    }

    [MapperIgnoreTarget(nameof(VideoContentInfo.Url))]
    [MapperIgnoreTarget(nameof(VideoContentInfo.SnapshotUrl))]
    [MapperIgnoreTarget(nameof(VideoContentInfo.SnapshotThumbnailUrl))]
    private partial void MapOutputFields(VideoContent source, VideoContentInfo destination);
}
