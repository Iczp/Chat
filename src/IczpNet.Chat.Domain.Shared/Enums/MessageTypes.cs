using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    [Description("消息类型")]
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
        Sound = 3,

        /// <summary>  
        /// 视频类型  
        /// </summary>  
        [Description("视频")]
        Video = 4,

        /// <summary>
        /// 文件消息
        /// </summary>
        [Description("文件消息")]
        File = 5,

        /// <summary>  
        /// 链接类型  
        /// </summary>  
        [Description("链接")]
        Link = 6,

        /// <summary>  
        /// 地理位置
        /// </summary>  
        [Description("地理位置")]
        Location = 7,

        /// <summary>  
        /// 联系人名片
        /// </summary>  
        [Description("联系人名片")]
        Contacts = 8,

        /// <summary>
        /// 红包
        /// </summary>
        [Description("红包")]
        RedEnvelope = 9,

        /// <summary>
        /// HTML
        /// </summary>
        [Description("HTML")]
        Html = 10,

        /// <summary>
        /// 文章
        /// </summary>
        [Description("文章")]
        Article = 11,

        /// <summary>
        /// 聊天历史消息
        /// </summary>
        [Description("聊天历史消息")]
        History = 12,
    }
}