using IczpNet.Chat.Enums;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.QrCodes;

public interface IQrCodeManager
{
    /// <summary>
    /// 生成或复用聊天对象名片/群聊邀请二维码
    /// </summary>
    Task<QrCode> GenerateCardAsync(
        long ownerId,
        long? inviterId = null,
        bool? isPermanent = null,
        int? expireDays = null,
        bool forceReset = false,
        string title = null,
        int? maxUseCount = null,
        string payloadJson = null);

    /// <summary>
    /// 生成通用场景二维码（如工作台应用分享、自定义业务码）
    /// </summary>
    Task<QrCode> GenerateCustomAsync(
        QrCodeScenes scene,
        long? ownerId = null,
        long? inviterId = null,
        bool? isPermanent = null,
        int? expireDays = null,
        bool forceReset = false,
        string title = null,
        int? maxUseCount = null,
        string payloadJson = null);

    /// <summary>
    /// 重置二维码（作废旧码与缓存并签发新码）
    /// </summary>
    Task<QrCode> ResetCardAsync(
        long ownerId,
        long? inviterId = null,
        bool? isPermanent = null,
        int? expireDays = null,
        string title = null,
        int? maxUseCount = null,
        string payloadJson = null);

    /// <summary>
    /// 停用指定二维码并清除缓存
    /// </summary>
    Task DisableAsync(Guid id);

    /// <summary>
    /// 通过短码获取缓存项
    /// </summary>
    Task<QrCodeCacheItem> GetCacheByCodeAsync(string code);

    /// <summary>
    /// 解析二维码原始内容并返回名片/群聊/扩展业务预览模型
    /// </summary>
    Task<QrCodeScanDataInfo> ResolveScanAsync(string rawContent, long? viewerChatObjectId = null);

    /// <summary>
    /// 根据短码直接解析预览模型
    /// </summary>
    Task<QrCodeScanDataInfo> ResolveByCodeAsync(string code, long? viewerChatObjectId = null);

    /// <summary>
    /// 执行扫码业务动作（加入群聊或添加好友/发起好友申请）
    /// </summary>
    Task<QrCodeApplyResult> ApplyAsync(string code, long applicantChatObjectId, string requestMessage = null);

    /// <summary>
    /// 格式化当前配置模板下的二维码内容
    /// </summary>
    Task<string> FormatContentAsync(string code);
}
