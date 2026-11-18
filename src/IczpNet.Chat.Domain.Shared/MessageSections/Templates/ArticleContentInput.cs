using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.MessageSections.Templates;

public class ArticleContentInput : MessageContentInfoBase, IContentInfo
{
    [Required, MinLength(1), MaxLength(8)]
    public List<ArticleItemInfo> Items { get; set; } = [];
}

public class ArticleContentInfo : ArticleContentInput
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string CoverImageUrl { get; set; }
    public string Author { get; set; }
    public string OriginalUrl { get; set; }
}
