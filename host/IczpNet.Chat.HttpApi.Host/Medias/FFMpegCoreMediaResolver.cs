using FFMpegCore;
using IczpNet.Chat.Medias;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace IczpNet.Chat;

// FFmpeg is a Chat host concern; the main IM host no longer supplies this resolver.
public class FFMpegCoreMediaResolver(IConfiguration configuration) : NullMediaResolver, IMediaResolver
{
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
