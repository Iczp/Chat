using Castle.DynamicProxy;
using IczpNet.AbpCommons.Extensions;
using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using IczpNet.Chat.MessageSections.MessageContents;
using IczpNet.Chat.MessageSections.Templates;
using IczpNet.Chat.RedEnvelopes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mime;

namespace IczpNet.Chat.MessageSections.Messages;

public partial class Message
{
    //public virtual dynamic Content => GetContent();
    public virtual dynamic Content => GetContentDto();

    public virtual Guid? MessageContentId { get; protected set; }

    [ForeignKey(nameof(MessageContentId))]
    public virtual MessageContent MessageContent { get; protected set; }


    [ContentType(MessageTypes.Cmd)]
    public virtual IList<CmdContent> CmdContentList { get; set; } = new List<CmdContent>();

    [ContentType(MessageTypes.Text)]
    public virtual IList<TextContent> TextContentList { get; set; } = new List<TextContent>();

    [ContentType(MessageTypes.Html)]
    public virtual IList<HtmlContent> HtmlContentList { get; set; } = new List<HtmlContent>();

    [ContentType(MessageTypes.Article)]
    public virtual IList<ArticleContent> ArticleContentList { get; set; } = new List<ArticleContent>();

    [ContentType(MessageTypes.Link)]
    public virtual IList<LinkContent> LinkContentList { get; set; } = new List<LinkContent>();

    [ContentType(MessageTypes.Image)]
    public virtual IList<ImageContent> ImageContentList { get; set; } = new List<ImageContent>();

    [ContentType(MessageTypes.Sound)]
    public virtual IList<SoundContent> SoundContentList { get; set; } = new List<SoundContent>();

    [ContentType(MessageTypes.Video)]
    public virtual IList<VideoContent> VideoContentList { get; set; } = new List<VideoContent>();

    [ContentType(MessageTypes.File)]
    public virtual IList<FileContent> FileContentList { get; set; } = new List<FileContent>();

    [ContentType(MessageTypes.Location)]
    public virtual IList<LocationContent> LocationContentList { get; set; } = new List<LocationContent>();

    [ContentType(MessageTypes.Contacts)]
    public virtual IList<ContactsContent> ContactsContentList { get; set; } = new List<ContactsContent>();

    [ContentType(MessageTypes.RedEnvelope)]
    public virtual IList<RedEnvelopeContent> RedEnvelopeContentList { get; set; } = new List<RedEnvelopeContent>();

    [ContentType(MessageTypes.History)]
    public virtual IList<HistoryContent> HistoryContentList { get; set; } = new List<HistoryContent>();

    //HistoryMessageList
    [InverseProperty(nameof(HistoryMessage.Message))]
    public virtual IList<HistoryMessage> HistoryMessageList { get; set; }

    public virtual dynamic GetContentEntity()
    {
        return this.GetMessageContent().ToDynamicList().FirstOrDefault();
    }

    public virtual IContentEntity GetTypedContentEntity()
    {
        return (IContentEntity)GetContentEntity();
    }

    public virtual object GetContentDto()
    {
        var content = GetContentEntity();

        var currentInstance = ProxyUtil.GetUnproxiedInstance(content);

        var currentType = ProxyUtil.GetUnproxiedType(content);

        var outputType = ContentOuputAttribute.GetOuputType(currentType);

        return AutoMapperExtensions.MapTo(currentInstance, currentType, outputType);
    }
}
