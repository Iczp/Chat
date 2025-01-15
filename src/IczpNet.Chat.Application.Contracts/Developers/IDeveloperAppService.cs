using IczpNet.Chat.Developers.Dtos;
using System.Threading.Tasks;

namespace IczpNet.Chat.Developers;

/// <summary>
/// 开发者
/// </summary>
public interface IDeveloperAppService
{
    /// <summary>
    /// 设置
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<DeveloperDto> SetConfigAsync(long id, ConfigInput input);

    /// <summary>
    /// 启用或禁用
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="isEnabled"></param>
    /// <returns></returns>
    Task<bool> SetIsEnabledAsync(long id, bool isEnabled);

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<EncryptOutput> EncryptAsync(EncryptInput input);

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<DecryptOutput> DecryptAsync(DecryptInput input);

    /// <summary>
    /// 生成签名
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<GenerateSignatureOutput> GenerateSignatureAsync(GenerateSignatureInput input);

    /// <summary>
    ///  验证签名
    /// </summary>
    Task<bool> VerifySignatureAsync(VerifySignatureInput input);

    /// <summary>
    /// String To Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> StringToBase64Async(string input);

    /// <summary>
    /// Base64 To String
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<string> Base64ToStringAsync(string input);

    /// <summary>
    /// 生成 EncodingAesKey
    /// </summary>
    /// <returns></returns>
    Task<string> GenerateEncodingAesKeyAsync();
}
