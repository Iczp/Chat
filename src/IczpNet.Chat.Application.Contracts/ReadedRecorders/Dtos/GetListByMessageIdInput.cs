using IczpNet.Chat.BaseDtos;
using System.ComponentModel;

namespace IczpNet.Chat.ReadedRecorders.Dtos
{
    public class GetListByMessageIdInput : BaseGetListInput
    {
        /// <summary>
        /// 是否已读
        /// </summary>
        [DefaultValue(true)]
        public virtual bool IsReaded { get; set; }
    }
}
