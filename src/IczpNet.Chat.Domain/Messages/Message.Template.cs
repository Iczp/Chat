using System.Collections.Generic;
using IczpNet.Chat.Messages.Templates;

namespace IczpNet.Chat.Messages
{
    public partial class Message
    {
        public virtual IList<CmdContent> CmdContentList { get; set; }
        public virtual IList<TextContent> TextContentList { get; set; }
        public virtual IList<HtmlContent> HtmlContentList { get; set; }
        public virtual IList<ArticleContent> ArticleContentList { get; set; }
        public virtual IList<LinkContent> LinkContentList { get; set; }
        public virtual IList<ImageContent> ImageContentList { get; set; }
        public virtual IList<SoundContent> SoundContentList { get; set; }
        public virtual IList<VideoContent> VideoContentList { get; set; }
        public virtual IList<FileContent> FileContentList { get; set; }
        public virtual IList<LocationContent> LocationContentList { get; set; }
        public virtual IList<ContactsContent> ContactsContentList { get; set; }
        public virtual IList<RedEnvelopeContent> RedEnvelopeContentList { get; set; }
        public virtual IList<HistoryContent> HistoryContentList { get; set; }

        //HistoryMessageList
        public virtual IList<HistoryMessage> HistoryMessageList { get; set; }
    }
}
