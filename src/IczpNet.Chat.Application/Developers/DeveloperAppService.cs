﻿using IczpNet.BizCrypts;
using IczpNet.Chat.BaseAppServices;
using IczpNet.Chat.Developers.Dtos;
using IczpNet.Chat.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.Developers;

/// <summary>
/// 开发者
/// </summary>
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

    /// <summary>
    /// 开发者设置
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="input"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 启用与关闭
    /// </summary>
    /// <param name="id">主建Id</param>
    /// <param name="isEnabled"></param>
    /// <returns></returns>
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
    /// 解密
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<DecryptOutput> DecryptAsync([FromQuery] DecryptInput input)
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
    public async Task<GenerateSignatureOutput> GenerateSignatureAsync([FromQuery] GenerateSignatureInput input)
    {
        string signature = BizCrypt.GenerateSignature(input.Token, input.TimeStamp, input.Nonce, input.Echo);

        return await Task.FromResult(new GenerateSignatureOutput()
        {
            Signature = signature
        });
    }

    /// <summary>
    /// 验证签名
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<bool> VerifySignatureAsync([FromQuery] VerifySignatureInput input)
    {
        var retult = BizCrypt.VerifySignature(input.Signature, input.Token, input.TimeStamp, input.Nonce, input.CipherText);

        return await Task.FromResult(retult);
    }

    /// <summary>
    /// String转Base64
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
    /// Base64转String
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
    /// 生成 EncodingAesKey(43)
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<string> GenerateEncodingAesKeyAsync()
    {
        return await Task.FromResult(BizCrypt.CreateRandCode(43));
    }
}
