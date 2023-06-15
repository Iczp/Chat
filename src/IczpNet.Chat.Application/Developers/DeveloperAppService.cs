using IczpNet.BizCrypts;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Developers.Dtos;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Developers
{
    public class DeveloperAppService : ChatAppService, IDeveloperAppService
    {
        protected virtual string SetIsEnabledPolicyName { get; set; } = ChatPermissions.DeveloperPermission.SetIsEnabled;

        public DeveloperAppService() { }

        [HttpGet]
        public async Task<DeveloperDto> GetAsync(long id)
        {
            var entity = await ChatObjectManager.GetAsync(id);

            if (entity.Developer == null)
            {
                return null;
            }
            return ObjectMapper.Map<Developer, DeveloperDto>(entity.Developer);
        }

        [HttpPost]
        public async Task<DeveloperDto> SetConfigAsync(long id, [FromQuery] ConfigInput input)
        {
            var entity = await ChatObjectManager.GetAsync(id);

            var developer = entity.Developer ??= new Developer();

            developer.Token = input.Token;

            developer.EncodingAesKey = input.EncodingAesKey;

            developer.PostUrl = input.PostUrl;

            return ObjectMapper.Map<Developer, DeveloperDto>(developer);
        }

        [HttpPost]
        public async Task<bool> SetIsEnabledAsync(long id, bool isEnabled)
        {
            await CheckPolicyAsync(SetIsEnabledPolicyName);

            var entity = await ChatObjectManager.GetAsync(id);

            entity.Developer.IsEnabled = isEnabled;

            return isEnabled;
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<EncryptOutput> EncryptAsync(EncryptInput input)
        {
            var bizCrypt = new BizCrypt(input.Token, input.EncodingAesKey, input.ChatObjectId);

            string encrypt = bizCrypt.Encrypt(input.EncryptData);

            return await Task.FromResult(new EncryptOutput()
            {
                ChatObjectId = input.ChatObjectId,
                EncodingAesKey = input.EncodingAesKey,
                Token = input.Token,
                Echo = encrypt
            });
        }

        /// <summary>
        /// 解密(失败时 EncryptData=Null)
        /// </summary>
        /// <param name="input"></param>
        /// <returns>失败 EncryptData=Null</returns>
        [HttpPost]
        public async Task<DecryptOutput> DecryptAsync(DecryptInput input)
        {
            var bizCrypt = new BizCrypt(input.Token, input.EncodingAesKey, input.ChatObjectId);

            string encrypt = bizCrypt.Decrypt(input.Echo);

            return await Task.FromResult(new DecryptOutput()
            {
                ChatObjectId = input.ChatObjectId,
                EncodingAesKey = input.EncodingAesKey,
                Token = input.Token,
                EncryptData = encrypt
            });
        }

        /// <summary>
        /// 生成签名
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<GenerateSignatureOutput> GenerateSignature([FromQuery] GenerateSignatureInput input)
        {
            string signature = BizCrypt.GenerateSignature(input.Token, input.TimeStamp, input.Nonce, input.Echo);

            return await Task.FromResult(new GenerateSignatureOutput()
            {
                Signature = signature
            });
        }

        /// <summary>
        ///  验证签名
        /// </summary>
        /// <param name="signature">签名</param>
        /// <param name="token">公众平台上，开发者设置的Token</param>
        /// <param name="timeStamp">时间戳</param>
        /// <param name="nonce">随机数</param>
        /// <param name="cipherText">密文</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<bool> VerifySignature(string signature, string token, string timeStamp, string nonce, string cipherText)
        {
            var retult = BizCrypt.VerifySignature(signature, token, timeStamp, nonce, cipherText);

            return await Task.FromResult(retult);
        }

        /// <summary>
        /// String => Base64
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<string> StringToBase64Async(string input)
        {
            byte[] b = System.Text.Encoding.Default.GetBytes(input);

            return await Task.FromResult(Convert.ToBase64String(b));
        }

        /// <summary>
        /// Base64 => String
        /// </summary>
        /// <param name="base64String"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<string> Base64ToStringAsync(string base64String)
        {
            byte[] b = Convert.FromBase64String(base64String);

            var output = System.Text.Encoding.Default.GetString(b);

            return await Task.FromResult(output);
        }

        /// <summary>
        /// 生成 EncodingAesKey
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<string> GenerateEncodingAesKeyAsync()
        {
            return await Task.FromResult(BizCrypt.CreateRandCode(43));
        }
    }
}
