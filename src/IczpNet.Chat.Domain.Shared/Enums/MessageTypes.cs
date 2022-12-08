using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    public enum MessageTypes
    {
        /// <summary>  
        /// 文本类型  
        /// </summary>  
        [Description("文本")]
        Text = 0,

        /// <summary>  
        /// 系统命令消息
        /// </summary>  
        [Description("系统命令消息")]
        Cmd = 1,

        /// <summary>  
        /// 图片类型  
        /// </summary>  
        [Description("图片")]
        Image = 2,

        /// <summary>  
        /// 语音类型  
        /// </summary>  
        [Description("语音")]
        Sound = 4,

        /// <summary>  
        /// 视频类型  
        /// </summary>  
        [Description("视频")]
        Video = 6,

        /// <summary>
        /// 文件消息
        /// </summary>
        [Description("文件消息")]
        File = 7,

        /// <summary>  
        /// 链接类型  
        /// </summary>  
        [Description("链接")]
        Link = 8,

        /// <summary>  
        /// 地理位置
        /// </summary>  
        [Description("地理位置")]
        Location = 9,

        /// <summary>  
        /// 联系人名片
        /// </summary>  
        [Description("联系人名片")]
        Contacts = 10,

        /// <summary>
        /// 红包
        /// </summary>
        [Description("红包")]
        RedEnvelope = 11,

        /// <summary>
        /// HTML
        /// </summary>
        [Description("HTML")]
        Html = 12,

        /// <summary>
        /// 文章
        /// </summary>
        [Description("文章")]
        Article = 13,

        /// <summary>
        /// 聊天历史消息
        /// </summary>
        [Description("聊天历史消息")]
        History = 14,
    }
}