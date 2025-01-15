using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.FavoritedRecorders.Dtos;

public class FavoritedRecorderInput : FavoritedRecorderCreateInput
{
    /// <summary>
    /// 是否收藏(True:收藏,False:取消收藏)
    /// </summary>
    [Required]
    public virtual bool IsFavorite { get; set; }
}
