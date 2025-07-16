using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Blobs;

public interface IBlobResolver
{
    /// <summary>
    /// 生成文件名
    /// </summary>
    /// <param name="suffix"></param>
    /// <returns></returns>
    Task<string> GenerateFileNameAsync(string suffix);

    /// <summary>
    /// 获取目录名称
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task<string> GetDirectoryNameAsync(BlobContext context);
    

    /// <summary>
    /// 获取文件Url
    /// </summary>
    /// <param name="blobId"></param>
    /// <returns></returns>
    Task<string> GetFileUrlAsync(Guid blobId);
}
