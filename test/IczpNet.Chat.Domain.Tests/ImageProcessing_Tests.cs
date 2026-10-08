using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IczpNet.AbpCommons;
using IczpNet.AbpCommons.Imaging;
using IczpNet.AbpCommons.Utils;
using ImageMagick;
using ImageMagick.Drawing;
using Microsoft.Extensions.Options;
using Volo.Abp.Imaging;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Xunit;
using Assert = Xunit.Assert;

namespace IczpNet.Chat;

[DependsOn(typeof(AbpCommonsDomainModule))]
public class ImageProcessingTestModule : AbpModule;

public class ImageProcessing_Tests : AbpIntegratedTest<ImageProcessingTestModule>
{
    private static byte[] CreateImage(MagickFormat format, uint width = 160, uint height = 80)
    {
        using var image = new MagickImage(MagickColors.Blue, width, height);
        return image.ToByteArray(format);
    }

    private static byte[] CreateAnimation()
    {
        using var frames = new MagickImageCollection();
        frames.Add(new MagickImage(MagickColors.Red, 80, 40) { AnimationDelay = 6, AnimationIterations = 0 });
        frames.Add(new MagickImage(MagickColors.Blue, 80, 40) { AnimationDelay = 11 });
        return frames.ToByteArray(MagickFormat.Gif);
    }

    private static (byte R, byte G, byte B, byte A) Pixel(MagickImage image, int x, int y)
    {
        using var pixels = image.GetPixels();
        var color = pixels.GetPixel(x, y).ToColor();
        return (color.R, color.G, color.B, color.A);
    }

    [Fact]
    public void Commons_module_resolves_magick_adapters_for_the_existing_abp_interfaces()
    {
        Assert.IsType<MagickImageResizer>(GetRequiredService<IImageResizer>());
        Assert.IsType<MagickImageCompressor>(GetRequiredService<IImageCompressor>());
        var options = GetRequiredService<IOptions<MagickImageOptions>>().Value;
        Assert.True(options.MaxInputBytes > 0);
        Assert.True(options.MaxDecodedPixels > 0);
        Assert.True(options.MaxFrames > 0);
    }

    [Theory]
    [InlineData(MagickFormat.Png)]
    [InlineData(MagickFormat.Jpeg)]
    [InlineData(MagickFormat.WebP)]
    [InlineData(MagickFormat.Gif)]
    [InlineData(MagickFormat.Bmp)]
    [InlineData(MagickFormat.Tiff)]
    public async Task Abp_resize_preserves_supported_formats_aspect_ratio_and_decodable_pixels(MagickFormat format)
    {
        var result = await GetRequiredService<IImageResizer>().ResizeAsync(CreateImage(format),
            new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.Max });
        Assert.Equal(ImageProcessState.Done, result.State);
        using var resized = new MagickImage(result.Result);
        Assert.Equal(40u, resized.Width);
        Assert.Equal(20u, resized.Height);
        Assert.Equal(format, resized.Format);
        var center = Pixel(resized, 20, 10);
        Assert.True(center.B > 230 && center.R < 25 && center.G < 25);
    }

    [Theory]
    [InlineData(ImageResizeMode.Max, 40, 20)]
    [InlineData(ImageResizeMode.Min, 80, 40)]
    [InlineData(ImageResizeMode.Crop, 40, 40)]
    [InlineData(ImageResizeMode.Pad, 40, 40)]
    [InlineData(ImageResizeMode.BoxPad, 40, 40)]
    [InlineData(ImageResizeMode.Stretch, 40, 40)]
    public async Task Abp_resize_modes_preserve_their_geometry(ImageResizeMode mode, uint expectedWidth, uint expectedHeight)
    {
        var result = await GetRequiredService<IImageResizer>().ResizeAsync(CreateImage(MagickFormat.Png),
            new ImageResizeArgs { Width = 40, Height = 40, Mode = mode });
        using var resized = new MagickImage(result.Result);
        Assert.Equal(expectedWidth, resized.Width);
        Assert.Equal(expectedHeight, resized.Height);
        Assert.Equal((byte)255, Pixel(resized, (int)resized.Width / 2, (int)resized.Height / 2).B);
        if (mode is ImageResizeMode.Pad or ImageResizeMode.BoxPad)
            Assert.Equal((byte)0, Pixel(resized, 0, 0).A);
    }

    [Fact]
    public async Task Box_pad_keeps_a_smaller_source_at_its_original_size_in_a_transparent_canvas()
    {
        var result = await GetRequiredService<IImageResizer>().ResizeAsync(CreateImage(MagickFormat.Png, 16, 8),
            new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.BoxPad });
        using var resized = new MagickImage(result.Result);
        Assert.Equal(40u, resized.Width);
        Assert.Equal(40u, resized.Height);
        Assert.Equal((byte)0, Pixel(resized, 11, 20).A);
        Assert.Equal((byte)255, Pixel(resized, 12, 20).A);
        Assert.Equal((byte)255, Pixel(resized, 27, 20).A);
        Assert.Equal((byte)0, Pixel(resized, 28, 20).A);
    }

    [Fact]
    public async Task None_returns_original_bytes_and_default_obeys_abp_resize_options()
    {
        var original = CreateImage(MagickFormat.Png);
        var resizer = GetRequiredService<IImageResizer>();
        var canceled = await resizer.ResizeAsync(original,
            new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.None });
        Assert.Equal(ImageProcessState.Canceled, canceled.State);
        Assert.Equal(original, canceled.Result);
        GetRequiredService<IOptions<ImageResizeOptions>>().Value.DefaultResizeMode = ImageResizeMode.Max;
        var result = await resizer.ResizeAsync(original,
            new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.Default });
        using var resized = new MagickImage(result.Result);
        Assert.Equal(40u, resized.Width);
        Assert.Equal(20u, resized.Height);
    }

    [Theory]
    [InlineData(MagickFormat.Png)]
    [InlineData(MagickFormat.Jpeg)]
    [InlineData(MagickFormat.WebP)]
    [InlineData(MagickFormat.Gif)]
    [InlineData(MagickFormat.Bmp)]
    [InlineData(MagickFormat.Tiff)]
    public async Task Abp_compression_preserves_format_dimensions_and_returns_no_larger_data(MagickFormat format)
    {
        var original = CreateImage(format);
        var result = await GetRequiredService<IImageCompressor>().CompressAsync(original);
        Assert.Contains(result.State, new[] { ImageProcessState.Done, ImageProcessState.Canceled });
        Assert.True(result.Result.Length <= original.Length);
        using var compressed = new MagickImage(result.Result);
        Assert.Equal(160u, compressed.Width);
        Assert.Equal(80u, compressed.Height);
        Assert.Equal(format, compressed.Format);
    }

    [Fact]
    public async Task Exif_orientation_is_applied_before_resize_and_existing_profile_fields_are_preserved()
    {
        byte[] original;
        using (var image = new MagickImage(MagickColors.Red, 16, 8))
        {
            new Drawables().FillColor(MagickColors.Blue).Rectangle(8, 0, 15, 7).Draw(image);
            var exif = new ExifProfile();
            exif.SetValue(ExifTag.Orientation, (ushort)6);
            exif.SetValue(ExifTag.Make, "IczpNet test camera");
            exif.SetValue(ExifTag.ImageDescription, "Chat profile metadata");
            image.SetProfile(exif);
            image.Orientation = OrientationType.RightTop;
            original = image.ToByteArray(MagickFormat.Jpeg);
        }
        using (var encoded = new MagickImage(original))
            Assert.Equal((ushort)6, encoded.GetExifProfile().GetValue(ExifTag.Orientation).Value);
        var result = await GetRequiredService<IImageResizer>().ResizeAsync(original,
            new ImageResizeArgs { Width = 16, Height = 16, Mode = ImageResizeMode.Max });
        using var oriented = new MagickImage(result.Result);
        Assert.Equal(8u, oriented.Width);
        Assert.Equal(16u, oriented.Height);
        var orientedExif = oriented.GetExifProfile();
        Assert.NotNull(orientedExif);
        Assert.Equal((ushort)1, orientedExif.GetValue(ExifTag.Orientation).Value);
        Assert.Equal("IczpNet test camera", orientedExif.GetValue(ExifTag.Make).Value);
        Assert.Equal("Chat profile metadata", orientedExif.GetValue(ExifTag.ImageDescription).Value);
        var top = Pixel(oriented, 4, 2);
        var bottom = Pixel(oriented, 4, 13);
        Assert.True(top.R > top.B + 100);
        Assert.True(bottom.B > bottom.R + 100);
    }

    [Fact]
    public async Task Gif_resize_and_compression_preserve_frames_delays_and_loop_setting()
    {
        var original = CreateAnimation();
        var result = await GetRequiredService<IImageResizer>().ResizeAsync(original,
            new ImageResizeArgs { Width = 20, Height = 20, Mode = ImageResizeMode.Max });
        using var resized = new MagickImageCollection(result.Result);
        Assert.Equal(2, resized.Count);
        Assert.All(resized, frame => { Assert.Equal(20u, frame.Width); Assert.Equal(10u, frame.Height); });
        Assert.Equal(new uint[] { 6, 11 }, resized.Select(x => x.AnimationDelay));
        Assert.Equal(0u, resized[0].AnimationIterations);
        var compressed = await GetRequiredService<IImageCompressor>().CompressAsync(original);
        using var frames = new MagickImageCollection(compressed.Result);
        Assert.Equal(2, frames.Count);
        Assert.Equal(new uint[] { 6, 11 }, frames.Select(x => x.AnimationDelay));
    }

    [Fact]
    public void Empty_png_and_non_square_round_corners_preserve_alpha_and_center_pixels()
    {
        using var empty = new MagickImage(ImageHelper.GetEmptyPngBytes(64, 24));
        Assert.Equal(64u, empty.Width);
        Assert.Equal(24u, empty.Height);
        Assert.Equal((byte)0, Pixel(empty, 32, 12).A);
        using var rounded = new MagickImage(ImageHelper.SetImageRoundCorner(CreateImage(MagickFormat.Png, 64, 24), 8));
        Assert.Equal(64u, rounded.Width);
        Assert.Equal(24u, rounded.Height);
        Assert.Equal((byte)0, Pixel(rounded, 0, 0).A);
        Assert.Equal((byte)0, Pixel(rounded, 63, 0).A);
        Assert.Equal((byte)0, Pixel(rounded, 0, 23).A);
        Assert.Equal((byte)0, Pixel(rounded, 63, 23).A);
        Assert.Equal(((byte)0, (byte)0, (byte)255, (byte)255), Pixel(rounded, 32, 12));
        Assert.Equal((byte)255, Pixel(rounded, 32, 0).A);
        using var unchanged = new MagickImage(ImageHelper.SetImageRoundCorner(CreateImage(MagickFormat.Png, 64, 24), 0));
        Assert.Equal(((byte)0, (byte)0, (byte)255, (byte)255), Pixel(unchanged, 0, 0));
    }

    [Fact]
    public void Collage_places_distinct_non_square_sources_in_cells_with_transparent_gaps_and_round_corners()
    {
        using var red = new MagickImage(MagickColors.Red, 24, 64);
        var bytes = ImageHelper.MakeMergeThumbnails(
            [CreateImage(MagickFormat.Png, 64, 24), red.ToByteArray(MagickFormat.Png)], 120, 6, 8);
        using var merged = new MagickImage(bytes);
        Assert.Equal(120u, merged.Width);
        Assert.Equal(120u, merged.Height);
        Assert.Equal(((byte)0, (byte)0, (byte)255, (byte)255), Pixel(merged, 22, 22));
        Assert.Equal(((byte)255, (byte)0, (byte)0, (byte)255), Pixel(merged, 60, 22));
        Assert.Equal((byte)0, Pixel(merged, 0, 0).A);
        Assert.Equal((byte)0, Pixel(merged, 6, 6).A);
        Assert.Equal((byte)0, Pixel(merged, 42, 22).A);
        Assert.Equal((byte)0, Pixel(merged, 98, 22).A);
        Assert.Equal((byte)0, Pixel(merged, 22, 60).A);
    }

    [Fact]
    public void Rounded_helper_preserves_existing_semitransparent_center_alpha()
    {
        using var source = new MagickImage(new MagickColor(20, 40, 200, 128), 64, 24);
        using var rounded = new MagickImage(ImageHelper.SetImageRoundCorner(source.ToByteArray(MagickFormat.Png), 8));
        Assert.Equal(((byte)20, (byte)40, (byte)200, (byte)128), Pixel(rounded, 32, 12));
        Assert.Equal((byte)0, Pixel(rounded, 0, 0).A);
    }

    [Theory]
    [InlineData("image/svg+xml")]
    [InlineData("application/pdf")]
    public async Task Unsupported_mime_types_do_not_process_otherwise_valid_png_bytes(string mimeType)
    {
        var original = CreateImage(MagickFormat.Png);
        var resized = await GetRequiredService<IImageResizer>().ResizeAsync(original,
            new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.Max }, mimeType);
        Assert.Equal(ImageProcessState.Unsupported, resized.State);
        Assert.Equal(original, resized.Result);
        var compressed = await GetRequiredService<IImageCompressor>().CompressAsync(original, mimeType);
        Assert.Equal(ImageProcessState.Unsupported, compressed.State);
        Assert.Equal(original, compressed.Result);
    }

    [Theory]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><rect width='100' height='100'/></svg>")]
    [InlineData("%PDF-1.4\n")]
    [InlineData("https://example.com/image.png")]
    [InlineData("file:///C:/Windows/win.ini")]
    [InlineData("not-an-image")]
    public async Task Unsupported_signatures_are_returned_without_invoking_external_coders(string data)
    {
        var original = Encoding.UTF8.GetBytes(data);
        var resized = await GetRequiredService<IImageResizer>().ResizeAsync(original,
            new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.Max });
        Assert.Equal(ImageProcessState.Unsupported, resized.State);
        Assert.Equal(original, resized.Result);
        var compressed = await GetRequiredService<IImageCompressor>().CompressAsync(original);
        Assert.Equal(ImageProcessState.Unsupported, compressed.State);
        Assert.Equal(original, compressed.Result);
    }

    [Fact]
    public async Task Recognized_but_truncated_input_fails_decoding_and_limits_reject_bounded_test_images()
    {
        var original = CreateImage(MagickFormat.Png);
        var resizer = GetRequiredService<IImageResizer>();
        var args = new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.Max };
        await Assert.ThrowsAnyAsync<MagickException>(() => resizer.ResizeAsync(original.Take(16).ToArray(), args));
        var options = GetRequiredService<IOptions<MagickImageOptions>>().Value;
        var inputLimit = options.MaxInputBytes;
        options.MaxInputBytes = original.Length - 1;
        await Assert.ThrowsAsync<InvalidDataException>(() => resizer.ResizeAsync(original, args));
        options.MaxInputBytes = inputLimit;
        var pixelLimit = options.MaxDecodedPixels;
        options.MaxDecodedPixels = 100;
        await Assert.ThrowsAsync<InvalidDataException>(() => resizer.ResizeAsync(original, args));
        options.MaxDecodedPixels = pixelLimit;
        options.MaxFrames = 1;
        await Assert.ThrowsAsync<InvalidDataException>(() => resizer.ResizeAsync(CreateAnimation(), args));
    }

    [Fact]
    public async Task Malformed_big_tiff_with_five_billion_directory_entries_is_rejected_within_five_seconds()
    {
        // Valid little-endian BigTIFF header, followed by a huge IFD entry count and no directory data.
        var malformed = new byte[24];
        malformed[0] = malformed[1] = (byte)'I';
        BinaryPrimitives.WriteUInt16LittleEndian(malformed.AsSpan(2, 2), 43);
        BinaryPrimitives.WriteUInt16LittleEndian(malformed.AsSpan(4, 2), 8);
        BinaryPrimitives.WriteUInt64LittleEndian(malformed.AsSpan(8, 8), 16);
        BinaryPrimitives.WriteUInt64LittleEndian(malformed.AsSpan(16, 8), 5_000_000_000);
        var resizer = GetRequiredService<IImageResizer>();
        var timer = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<MagickException>(() => Task.Run(() => resizer.ResizeAsync(malformed,
                new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.Max }, "image/tiff"))
            .WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.True(timer.Elapsed < TimeSpan.FromSeconds(5), $"Malformed BigTIFF rejection took {timer.Elapsed}.");
    }

    [Fact]
    public async Task Stream_contract_keeps_input_open_returns_rewound_output_and_honors_cancellation()
    {
        using var input = new MemoryStream(CreateImage(MagickFormat.Png));
        var resizer = GetRequiredService<IImageResizer>();
        var args = new ImageResizeArgs { Width = 40, Height = 40, Mode = ImageResizeMode.Max };
        var result = await resizer.ResizeAsync(input, args);
        using var output = result.Result;
        Assert.True(input.CanRead);
        Assert.Equal(0, output.Position);
        using var decoded = new MagickImage(output);
        Assert.Equal(40u, decoded.Width);
        input.Position = 0;
        var compressed = await GetRequiredService<IImageCompressor>().CompressAsync(input);
        using var compressedOutput = compressed.Result;
        Assert.True(input.CanRead);
        Assert.Equal(0, compressedOutput.Position);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => resizer.ResizeAsync(
            CreateImage(MagickFormat.Png), args, cancellationToken: cancellation.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => GetRequiredService<IImageCompressor>().CompressAsync(
            CreateImage(MagickFormat.Png), cancellationToken: cancellation.Token));
    }
}
