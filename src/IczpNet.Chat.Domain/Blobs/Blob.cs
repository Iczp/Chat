using IczpNet.AbpCommons.DataFilters;
using IczpNet.Chat.BaseEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace IczpNet.Chat.Blobs
{
    [Index(nameof(Container))]
    [Index(nameof(Name))]
    [Index(nameof(MimeType))]
    [Index(nameof(Suffix))]
    public class Blob : BaseEntity<Guid>, IIsPublic, IIsStatic
    {
        /// <summary>
        /// 容器
        /// </summary>
        [MaxLength(50)]
        [Required]
        public virtual string Container { get; protected set; }

        /// <summary>
        /// Blob名称
        /// </summary>
        [MaxLength(500)]
        public virtual string Name { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        [MaxLength(256)]
        public virtual string FileName { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public virtual long FileSize { get; set; }

        /// <summary>
        /// 类型 ContentType
        /// </summary>
        [MaxLength(100)]
        public virtual string MimeType { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        [MaxLength(10)]
        public virtual string Suffix { get; set; }

        /// <summary>
        /// IsPublic
        /// </summary>
        public virtual bool IsPublic { get; set; }

        /// <summary>
        /// IsStatic
        /// </summary>
        public virtual bool IsStatic { get; set; }

        ///// <summary>
        ///// 内容
        ///// </summary>
        //[NotMapped]
        //public virtual byte[] Content { get; set; }

        public virtual BlobContent Content { get; set; } = new BlobContent();

        public Blob() { }

        public Blob(Guid id, string container) : base(id)
        {
            Container = container;
        }
    }
}
