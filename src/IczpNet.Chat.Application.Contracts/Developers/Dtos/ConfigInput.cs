using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Developers.Dtos
{
    public class ConfigInput
    {
        /// <summary>
        /// 开发者设置的Token
        /// </summary>
        [StringLength(50)]
        [DefaultValue("95fd7796cc15d9f81f3f79dbc090ab03fb2941ef")]
        public virtual string Token { get; set; }

        /// <summary>
        /// 开发者设置的EncodingAESKey
        /// </summary>
        [StringLength(43)]
        [DefaultValue("GUhGDQKNcRpnp4XHQtnJY24MXWmMGV64KtahF3tjUQd")]
        public virtual string EncodingAesKey { get; set; }

        /// <summary>
        /// 开发者设置 的 Url
        /// </summary>
        [StringLength(256)]
        [DefaultValue("https://")]
        public virtual string PostUrl { get; set; }

    }
}
