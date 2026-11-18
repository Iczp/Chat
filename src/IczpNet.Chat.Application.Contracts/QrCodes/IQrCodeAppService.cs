using IczpNet.Chat.QrCodes.Dtos;
using System;
using System.Threading.Tasks;

namespace IczpNet.Chat.QrCodes;

public interface IQrCodeAppService : ICrudChatAppService<
    QrCodeDetailDto,
    QrCodeDto,
    Guid,
    QrCodeGetListInput>
{
    /// <summary>
    /// 获取或生成名片/群聊/自定义二维码
    /// </summary>
    Task<QrCodeDetailDto> GenerateAsync(QrCodeGenerateInput input);

    /// <summary>
    /// 重置二维码（作废旧码并签发新码）
    /// </summary>
    Task<QrCodeDetailDto> ResetAsync(QrCodeGenerateInput input);

    /// <summary>
    /// 根据短码获取二维码摘要及名片详情
    /// </summary>
    Task<QrCodeScanDataInfo> GetByCodeAsync(string code, long? currentChatObjectId = null);

    /// <summary>
    /// 解析完整二维码内容（支持配置模板、HTTPS短链及历史URI）
    /// </summary>
    Task<QrCodeScanDataInfo> ResolveAsync(string content, long? currentChatObjectId = null);

    /// <summary>
    /// 扫码执行加好友或入群操作
    /// </summary>
    Task<QrCodeApplyOutput> ApplyAsync(QrCodeApplyInput input);

    /// <summary>
    /// 停用指定二维码
    /// </summary>
    Task DisableAsync(Guid id);
}
