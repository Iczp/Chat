using System.ComponentModel;

namespace IczpNet.Chat.Enums
{
    /// <summary>
    /// 消息类型，0:其他未知,1:文本,2:图片,3:语音 4:视频,5:链接,6:工作流,7:地理位置,8:联系人名片,9:系统消息,10:课程,11:红包,12:Html,13:文章,14:聊天历史消息,15:文件消息
    /// </summary>
    public enum MessageTypes
    {
        /// <summary>  
        /// 其他未知  
        /// </summary>  
        [Description("其他未知")]
        Undefined = 0,
        /// <summary>  
        /// 文本类型  
        /// </summary>  
        [Description("文本")]
        Text = 1,
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
        /// 链接类型  
        /// </summary>  
        [Description("链接")]
        Link = 5,
        /// <summary>  
        /// 工作流类型  
        /// </summary>  
        [Description("工作流")]
        WordFlow = 6,
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
        /// 系统命令消息
        /// </summary>  
        [Description("系统命令消息")]
        Cmd = 9,
        /// <summary>  
        /// 课程
        /// </summary>  
        [Description("课程")]
        Course = 10,
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
        /// <summary>
        /// 文件消息
        /// </summary>
        [Description("文件消息")]
        File = 15,
        /// <summary>
        /// 通知消息（公告，提醒）
        /// </summary>
        [Description("通知消息")]
        Notice = 16,
    }
}