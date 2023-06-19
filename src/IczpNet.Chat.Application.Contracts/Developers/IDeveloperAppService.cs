using IczpNet.Chat.Developers.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IczpNet.Chat.Developers
{
    public interface IDeveloperAppService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        Task<DeveloperDto> SetConfigAsync(long id, ConfigInput input);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
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
        Task<GenerateSignatureOutput> GenerateSignature(GenerateSignatureInput input);

        /// <summary>
        ///  验证签名
        /// </summary>
        /// <param name="signature">签名 signature</param>
        /// <param name="token">公众平台上，开发者设置的Token</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="cipherText">密文</param>
        /// <returns></returns>
        Task<bool> VerifySignature(string signature, string token, string timeStamp, string nonce, string cipherText);

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
}
