using Volo.Abp.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace IczpNet.Chat.HttpRequests
{
    [Index(nameof(HttpMethod))]
    [Index(nameof(Scheme))]
    [Index(nameof(Host))]
    [Index(nameof(Port))]
    [Index(nameof(AbsolutePath))]
    [Index(nameof(StatusCode))]
    [Index(nameof(IsSuccess))]
    public class HttpRequest : Entity<Guid>
    {
        public static string ClientName => nameof(HttpRequest);

        [MaxLength(100)]
        public virtual string Name { get; set; }
        /// <summary>
        /// 请求方式
        /// </summary>
        [Column(TypeName = "nvarchar(10)")]
        public virtual string HttpMethod { get; set; }

        [MaxLength(500)]
        public string Host { get; internal set; }

        [MaxLength(10)]
        public virtual string Scheme { get; internal set; }

        public virtual int Port { get; internal set; }

        public virtual bool IsDefaultPort { get; internal set; }

        [MaxLength(500)]
        public virtual string Query { get; internal set; }

        [MaxLength(500)]
        public virtual string Fragment { get; internal set; }

        [MaxLength(500)]
        public string AbsolutePath { get; internal set; }

        /// <summary>
        /// Url
        /// </summary>
        //[MaxLength(5000)]
        public virtual string Url { get; set; }

        /// <summary>
        /// Parameters
        /// </summary>
        //[MaxLength(5000)]
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
        /// Message
        /// </summary>
        public virtual string Message { get; set; }

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
        /// Response content length
        /// </summary>
        public virtual int ContentLength { get; set; }

        public virtual HttpResponse Response { get; set; }

        /// <summary>
        /// ms
        /// </summary>
        public virtual long Duration { get; internal set; }
        



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
