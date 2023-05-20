using IczpNet.Chat.BaseEntitys;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Badwords
{
    [Index(nameof(Word), AllDescending = true)]
    public class Badword //: BaseEntity<string>
    {
        [MaxLength(36)]
        public virtual string Word { get; set; }

        public virtual bool IsEnabled { get; set; }

        protected Badword() { }
    }
}
