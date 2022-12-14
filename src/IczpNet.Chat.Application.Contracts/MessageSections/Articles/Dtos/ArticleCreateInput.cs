using IczpNet.Chat.BaseDtos;
using IczpNet.Chat.Enums;

namespace IczpNet.Chat.Articles.Dtos;

public class ArticleCreateInput : BaseInput
{
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
    //[Index]
    public virtual string Title { get; set; }

    /// <summary>
    /// 简要说明
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// 封面图片地址
    /// </summary>
    public virtual string CoverImageUrl { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public virtual string Content { get; set; }

    /// <summary>
    /// 作者
    /// </summary>

    public virtual string Author { get; set; }

    /// <summary>
    /// 访问数量
    /// </summary>
    public virtual long VisitsCount { get; set; }

    /// <summary>
    /// 原始地址
    /// </summary>
    public virtual string OriginalUrl { get; set; }
}
