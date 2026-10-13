using IczpNet.Chat.Attributes;
using IczpNet.Chat.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using Volo.Abp;

namespace IczpNet.Chat.MessageSections.Templates;

[MessageTemplate(MessageTypes.Article)]
[ContentOuput(typeof(ArticleContentInfo))]
public class ArticleContent : MessageContentEntityBase
{
    public override long GetSize() => Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(GetItems()));

    private const string ItemsProperty = "ArticleItems";

    // An ordered snapshot in the existing ExtraProperties column keeps legacy rows readable.
    public List<ArticleItemInfo> GetItems()
    {
        if (ExtraProperties.TryGetValue(ItemsProperty, out var value) && value != null)
            return JsonSerializer.Deserialize<List<ArticleItemInfo>>(value.ToString()) ?? [];
        return [new ArticleItemInfo { ArticleType = ArticleType, EditorType = EditorType, Title = Title,
            Description = Description, CoverImageUrl = CoverImageUrl, Author = Author, OriginalUrl = OriginalUrl }];
    }

    public static ArticleContent Create(ArticleContentInput input)
    {
        if (input?.Items == null || input.Items.Count is < 1 or > 8)
            throw new UserFriendlyException("每组文章必须包含 1～8 篇文章。");
        foreach (var item in input.Items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Title) || item.Title.Length > 256 ||
                (item.Description?.Length ?? 0) > 256 || (item.Author?.Length ?? 0) > 50 ||
                (item.OriginalUrl?.Length ?? 0) > 500 || (item.CoverImageUrl?.Length ?? 0) > 500 ||
                !IsWebUrl(item.OriginalUrl) || (!string.IsNullOrEmpty(item.CoverImageUrl) && !IsWebUrl(item.CoverImageUrl)))
                throw new UserFriendlyException("请检查文章标题、摘要、作者及 HTTP/HTTPS 阅读和封面地址。");
        }
        var first = input.Items[0];
        var result = new ArticleContent { ArticleType = first.ArticleType, EditorType = first.EditorType,
            Title = first.Title, Description = first.Description, CoverImageUrl = first.CoverImageUrl,
            Author = first.Author, OriginalUrl = first.OriginalUrl };
        result.ExtraProperties[ItemsProperty] = JsonSerializer.Serialize(input.Items);
        return result;
    }

    private static bool IsWebUrl(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) && !string.IsNullOrEmpty(uri.Host) && string.IsNullOrEmpty(uri.UserInfo);

    /// <summary>
    /// 文章类型
    /// </summary>
    public virtual ArticleTypes ArticleType { get; set; }

    /// <summary>
    /// 编辑器类型
    /// </summary>
    //[Index]
    public virtual EditorTypes EditorType { get; set; }

    /// <summary>
    /// 文本内容
    /// </summary>
    [Required(ErrorMessage = "文本内容[Title]必填！")]
    [StringLength(256)]
    //[Index]
    public virtual string Title { get; set; }

    /// <summary>
    /// 简要说明
    /// </summary>
    [StringLength(256)]
    //[Index]
    public virtual string Description { get; set; }

    /// <summary>
    /// 封面图片地址
    /// </summary>
    [StringLength(500)]
    public virtual string CoverImageUrl { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [StringLength(5000)]
    public virtual string Content { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [StringLength(50)]
    //[Index]
    public virtual string Author { get; set; }

    /// <summary>
    /// 访问数量
    /// </summary>
    public virtual long VisitsCount { get; set; }

    /// <summary>
    /// 原始地址
    /// </summary>
    [StringLength(500)]
    public virtual string OriginalUrl { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    [StringLength(36)]
    public virtual string CreatorUserId { get; set; }
}
