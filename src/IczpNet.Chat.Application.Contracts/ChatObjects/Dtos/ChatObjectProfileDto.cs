namespace IczpNet.Chat.ChatObjects.Dtos;

public class ChatObjectProfileDto
{
    /// <summary>
    /// ChatObject
    /// </summary>
    public virtual ChatObjectDto Owner { get; set; }

    /// <summary>
    /// 我的收藏数量
    /// </summary>
    public virtual int? FavoritedCount { get; set; }

    /// <summary>
    /// 我的关注数量
    /// </summary>
    public virtual int? FllowingCount { get; set; }

    /// <summary>
    /// 我的粉丝数量
    /// </summary>
    public virtual int? FllowerCount { get; set; }

}
