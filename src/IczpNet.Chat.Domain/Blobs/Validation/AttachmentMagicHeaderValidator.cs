using IczpNet.Chat.Options;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace IczpNet.Chat.Blobs.Validation;

/// <summary>
/// 校验常见图片/音视频文件头魔数，防止伪造扩展名或 Content-Type
/// </summary>
public class AttachmentMagicHeaderValidator(
    IOptions<AttachmentUploadOptions> options) : IAttachmentUploadValidator, ITransientDependency
{
    public int Order => 300;

    public async Task ValidateAsync(AttachmentUploadValidationContext context, CancellationToken cancellationToken = default)
    {
        if (!options.Value.EnableMagicHeaderValidation || context.MediaType == AttachmentMediaType.File)
        {
            return;
        }

        await using var stream = await context.OpenReadStreamAsync(cancellationToken);
        var buffer = new byte[16];
        var read = await ReadUpToAsync(stream, buffer, cancellationToken);
        if (read < 4)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileHeaderMismatch)
                .WithData("MediaType", context.MediaType);
        }

        var header = buffer.AsSpan(0, read);
        var valid = context.MediaType switch
        {
            AttachmentMediaType.Image or AttachmentMediaType.Snapshot => IsKnownImageHeader(header),
            AttachmentMediaType.Video => IsKnownVideoHeader(header),
            AttachmentMediaType.Sound => IsKnownAudioHeader(header),
            _ => true,
        };

        if (!valid)
        {
            throw new BusinessException(ChatErrorCodes.Blobs.FileHeaderMismatch)
                .WithData("MediaType", context.MediaType)
                .WithData("Suffix", context.Suffix);
        }
    }

    private static async Task<int> ReadUpToAsync(Stream stream, byte[] buffer, CancellationToken ct)
    {
        var total = 0;
        while (total < buffer.Length)
        {
            var n = await stream.ReadAsync(buffer.AsMemory(total, buffer.Length - total), ct);
            if (n == 0) break;
            total += n;
        }
        return total;
    }

    private static bool IsKnownImageHeader(ReadOnlySpan<byte> h)
    {
        // JPEG: FF D8 FF
        if (h.Length >= 3 && h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return true;
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (h.Length >= 8 && h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47) return true;
        // GIF: "GIF8"
        if (h.Length >= 4 && h[0] == 'G' && h[1] == 'I' && h[2] == 'F' && h[3] == '8') return true;
        // BMP: "BM"
        if (h.Length >= 2 && h[0] == 'B' && h[1] == 'M') return true;
        // WEBP: "RIFF"...."WEBP"
        if (h.Length >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
            && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P') return true;
        return false;
    }

    private static bool IsKnownVideoHeader(ReadOnlySpan<byte> h)
    {
        // MP4/MOV: ....ftyp
        if (h.Length >= 8 && h[4] == 'f' && h[5] == 't' && h[6] == 'y' && h[7] == 'p') return true;
        // WebM/MKV: 1A 45 DF A3
        if (h.Length >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3) return true;
        // AVI: "RIFF"...."AVI "
        if (h.Length >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
            && h[8] == 'A' && h[9] == 'V' && h[10] == 'I') return true;
        return false;
    }

    private static bool IsKnownAudioHeader(ReadOnlySpan<byte> h)
    {
        // MP3: "ID3" or FF FB / FF F3 / FF F2
        if (h.Length >= 3 && h[0] == 'I' && h[1] == 'D' && h[2] == '3') return true;
        if (h.Length >= 2 && h[0] == 0xFF && (h[1] & 0xE0) == 0xE0) return true;
        // WAV: "RIFF"...."WAVE"
        if (h.Length >= 12 && h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
            && h[8] == 'W' && h[9] == 'A' && h[10] == 'V' && h[11] == 'E') return true;
        // OGG / OPUS: "OggS"
        if (h.Length >= 4 && h[0] == 'O' && h[1] == 'g' && h[2] == 'g' && h[3] == 'S') return true;
        // M4A: ....ftyp
        if (h.Length >= 8 && h[4] == 'f' && h[5] == 't' && h[6] == 'y' && h[7] == 'p') return true;
        // AMR: "#!AMR"
        if (h.Length >= 5 && h[0] == '#' && h[1] == '!' && h[2] == 'A' && h[3] == 'M' && h[4] == 'R') return true;
        // WebM/Opus: 1A 45 DF A3
        if (h.Length >= 4 && h[0] == 0x1A && h[1] == 0x45 && h[2] == 0xDF && h[3] == 0xA3) return true;
        return false;
    }
}
