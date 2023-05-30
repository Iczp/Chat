using Volo.Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System;
using IczpNet.Chat.Enums;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.HttpRequests
{
    [Index(nameof(HttpMethod))]
    [Index(nameof(Url))]
    [Index(nameof(StatusCode))]
    [Index(nameof(IsSuccess))]
    public class HttpRequest : Entity<Guid>
    {
        /// <summary>
        /// 请求方式
        /// </summary>
        [Column(TypeName = "nvarchar(10)")]
        public virtual HttpMethods HttpMethod { get; set; }

        /// <summary>
        /// Url
        /// </summary>
        [MaxLength(500)]
        public virtual string Url { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        [MaxLength(5000)]
        public virtual string Parameters { get; set; }

        /// <summary>
        /// Timeout(秒)
        /// </summary>
        public virtual int Timeout { get; set; }

        /// <summary>
        /// UserAgent
        /// </summary>
        [StringLength(500)]
        public virtual string UserAgent { get; set; }

        /// <summary>
        /// Cookies
        /// </summary>
        [StringLength(500)]
        public virtual string Cookies { get; set; }

        /// <summary>
        /// Referer
        /// </summary>
        [StringLength(500)]
        public virtual string Referer { get; set; }

        /// <summary>
        /// Headers
        /// </summary>
        [MaxLength(500)]
        public virtual string Headers { get; set; }

        /// <summary>
        /// 是否请求成功
        /// </summary>
        public virtual bool IsSuccess { get; set; }

        /// <summary>
        /// ResponseContent
        /// </summary>
        public virtual string ResponseContent { get; set; }

        /// <inheritdoc/>
        public HttpStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// 请求开始时间
        /// </summary>
        public virtual long StartTime { get; set; }

        /// <summary>
        /// 请求结束时间
        /// </summary>
        public virtual long EndTime { get; set; }

        /// <summary>
        /// 构造器
        /// </summary>
        public HttpRequest()
        {
            Timeout = 30;
            //EndTime = Clock.Now.Ticks;
        }
        /// <summary>
        /// 获取执行时间
        /// </summary>
        /// <returns></returns>
        public string GetExeTime()
        {
            var n = (double)(EndTime - StartTime) / 10000;
            return string.Concat(n.ToString(), "ms");
        }

    }
}
