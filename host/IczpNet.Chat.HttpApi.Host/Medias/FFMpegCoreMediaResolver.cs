using FFMpegCore;
using IczpNet.Chat.Medias;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace IczpNet.Chat;

// FFmpeg is a Chat host concern; the main IM host no longer supplies this resolver.
public class FFMpegCoreMediaResolver(IConfiguration configuration) : NullMediaResolver, IMediaResolver
{
    public override async Task<AudioInfo> GetAudioInfoAsync(byte[] bytes, string fileName)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return null;
        }

        string tempAudioPath = null;
        try
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = configuration["Ffmpeg:BinaryFolder"] });
            var stopwatch = Stopwatch.StartNew();
            var suffix = Path.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(suffix))
            {
                suffix = ".audio";
            }

            var relativePath = Path.Combine(Clock.Now.ToString("yyyy/MM/dd"), Path.GetRandomFileName().Replace(".", "") + suffix);
            tempAudioPath = Path.Combine(Options.OutputTempPath, relativePath);
            var outputPath = Path.GetDirectoryName(tempAudioPath)!;
            Directory.CreateDirectory(outputPath);

            await using (var fileStream = new FileStream(tempAudioPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await fileStream.WriteAsync(bytes);
                await fileStream.FlushAsync();
            }

            var mediaInfo = await FFProbe.AnalyseAsync(tempAudioPath);
            var duration = mediaInfo.Duration > TimeSpan.Zero
                ? mediaInfo.Duration
                : mediaInfo.PrimaryAudioStream?.Duration ?? TimeSpan.Zero;

            if (duration <= TimeSpan.Zero)
            {
                return null;
            }

            var profile = new Dictionary<string, string>
            {
                ["Resolver"] = typeof(FFMpegCoreMediaResolver).FullName,
                ["ElapsedMilliseconds"] = stopwatch.ElapsedMilliseconds.ToString(),
            };

            if (!string.IsNullOrWhiteSpace(mediaInfo.Format?.FormatName))
            {
                profile["FormatName"] = mediaInfo.Format.FormatName;
            }

            var audioStream = mediaInfo.PrimaryAudioStream;
            if (audioStream != null)
            {
                if (!string.IsNullOrWhiteSpace(audioStream.CodecName))
                {
                    profile["CodecName"] = audioStream.CodecName;
                }
                if (audioStream.SampleRateHz > 0)
                {
                    profile["SampleRateHz"] = audioStream.SampleRateHz.ToString();
                }
                if (audioStream.Channels > 0)
                {
                    profile["Channels"] = audioStream.Channels.ToString();
                }
                if (audioStream.BitRate > 0)
                {
                    profile["BitRate"] = audioStream.BitRate.ToString();
                }
            }

            return new AudioInfo
            {
                Duration = duration,
                Profile = profile,
            };
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "GetAudioInfoAsync failed for {FileName}", fileName);
            return null;
        }
        finally
        {
            if (!string.IsNullOrWhiteSpace(tempAudioPath))
            {
                try
                {
                    if (File.Exists(tempAudioPath))
                    {
                        File.Delete(tempAudioPath);
                    }
                }
                catch (IOException cleanupException)
                {
                    Logger.LogWarning(cleanupException, "Could not delete temporary audio file {Path}", tempAudioPath);
                }
                catch (UnauthorizedAccessException cleanupException)
                {
                    Logger.LogWarning(cleanupException, "Could not delete temporary audio file {Path}", tempAudioPath);
                }
            }
        }
    }

    public override Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName)
        => GetVideoInfoAsync(bytes, fileName, new VideoProcessOptions());

    public override async Task<VideoInfo> GetVideoInfoAsync(byte[] bytes, string fileName, VideoProcessOptions options)
    {
        options ??= new VideoProcessOptions();
        string tempVideoPath = null;
        string gifSnapshotPath = null;
        string imgSnapshotPath = null;
        try
        {
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = configuration["Ffmpeg:BinaryFolder"] });
            var stopwatch = Stopwatch.StartNew();
            var suffix = Path.GetExtension(fileName);
            var relativePath = Path.Combine(Clock.Now.ToString("yyyy/MM/dd"), Path.GetRandomFileName().Replace(".", "") + suffix);
            tempVideoPath = Path.Combine(Options.OutputTempPath, relativePath);
            var outputPath = Path.GetDirectoryName(tempVideoPath)!;
            Directory.CreateDirectory(outputPath);

            await using var fileStream = new FileStream(tempVideoPath, FileMode.Create, FileAccess.Write);
            await fileStream.WriteAsync(bytes);
            await fileStream.FlushAsync();

            var mediaInfo = await FFProbe.AnalyseAsync(tempVideoPath);
            var tempFileName = Path.GetFileName(tempVideoPath);
            gifSnapshotPath = Path.Combine(outputPath, $"{tempFileName}.gif");
            imgSnapshotPath = Path.Combine(outputPath, $"{tempFileName}.png");
            var (width, height) = Options.VideoSetting.ImgSnapshotSize;
            var gifSize = Options.VideoSetting.GifSnapshotSize;
            var imageCreated = options.IncludeSnapshot && await FFMpeg.SnapshotAsync(tempVideoPath, imgSnapshotPath,
                new Size(width, height), TimeSpan.FromSeconds(Options.VideoSetting.ImgCaptureSeconds));
            var gifStartSeconds = Math.Min(Math.Max(Options.VideoSetting.ImgCaptureSeconds, 0),
                Math.Max(mediaInfo.Duration.TotalSeconds - 0.1, 0));
            var gifDurationSeconds = Math.Min(Math.Max(Options.VideoSetting.GifSeconds, 0),
                mediaInfo.Duration.TotalSeconds - gifStartSeconds);
            var gifCreated = options.IncludeGif && gifDurationSeconds > 0 && await FFMpeg.GifSnapshotAsync(tempVideoPath, gifSnapshotPath,
                new Size(gifSize.Width, gifSize.Height),
                captureTime: TimeSpan.FromSeconds(gifStartSeconds),
                duration: TimeSpan.FromSeconds(gifDurationSeconds));

            var isRotated = mediaInfo.PrimaryVideoStream != null &&
                            Math.Abs(mediaInfo.PrimaryVideoStream.Rotation) % 180 != 0;
            var vWidth = mediaInfo.PrimaryVideoStream?.Width ?? 0;
            var vHeight = mediaInfo.PrimaryVideoStream?.Height ?? 0;
            var resolvedVideoWidth = isRotated ? vHeight : vWidth;
            var resolvedVideoHeight = isRotated ? vWidth : vHeight;

            return new VideoInfo
            {
                Resolver = typeof(FFMpegCoreMediaResolver).FullName,
                Size = fileStream.Length,
                TempVideoPath = tempVideoPath,
                Duration = mediaInfo.Duration,
                Width = resolvedVideoWidth,
                Height = resolvedVideoHeight,
                ImageSnapshotPath = imageCreated ? imgSnapshotPath : null,
                GifSnapshotPath = gifCreated ? gifSnapshotPath : null,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception exception)
        {
            Logger.LogError(exception, "GetVideoInfoAsync failed for {FileName}", fileName);
            foreach (var path in new[] { imgSnapshotPath, gifSnapshotPath, tempVideoPath })
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                try { File.Delete(path); }
                catch (IOException cleanupException) { Logger.LogWarning(cleanupException, "Could not delete temporary media file {Path}", path); }
                catch (UnauthorizedAccessException cleanupException) { Logger.LogWarning(cleanupException, "Could not delete temporary media file {Path}", path); }
            }
            return null;
        }
    }
}
