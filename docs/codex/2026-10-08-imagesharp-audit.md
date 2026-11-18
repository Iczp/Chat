# 图片处理库迁移与安全告警修复

日期：2026-10-08。根据用户选择，将图片处理迁移至 Magick.NET-Q8-AnyCPU 14.17.2，并移除实际应用中的 ImageSharp 依赖链。

## 原因与依赖变化

原来的依赖来自 `IczpNet.AbpCommons.Domain 0.10.0` 内的 `SixLabors.ImageSharp.Drawing 2.1.4`、`Volo.Abp.Imaging.ImageSharp 10.6.1`，以及 Chat HttpApi 对 ABP ImageSharp 模块的直接引用，最终引入 ImageSharp 3.1.11。

曾验证直接升级到 ImageSharp 4.1.2，但 Release 构建需要有效的 SixLabors 许可证，且旧 Drawing 存在二进制兼容问题。用户确认无许可证并选择迁移。

现在 Commons Domain 使用 Magick.NET 和 `Volo.Abp.Imaging.Abstractions`，注册 `IImageResizer`、`IImageCompressor` 的替代实现。Chat HttpApi 和主 Host 使用这一实现，保留 ABP 接口和既有 API/DTO。移除了 ImageSharp 的包引用、模块引用、类型引用及旧的相关告警抑制项，没有关闭 NuGet 审计。

依赖检查确认 Chat HttpApi 与 Rctea.IM Host 均不再依赖 `SixLabors.ImageSharp`，因此以下五条 ImageSharp 告警的依赖根源已移除：

- [GHSA-gwg2-r3hj-4w44](https://github.com/advisories/GHSA-gwg2-r3hj-4w44)
- [GHSA-j3p4-wp97-rph4](https://github.com/advisories/GHSA-j3p4-wp97-rph4)
- [GHSA-j9gm-c75j-xc9q](https://github.com/advisories/GHSA-j9gm-c75j-xc9q)
- [GHSA-jjfr-hcj7-qf5w](https://github.com/advisories/GHSA-jjfr-hcj7-qf5w)
- [GHSA-wmxv-xphr-5c9g](https://github.com/advisories/GHSA-wmxv-xphr-5c9g)

## 功能与边界

- 支持 PNG、JPEG、WebP、GIF、BMP、TIFF。根据字节签名固定解码器，拒绝 SVG、PDF、路径、URL 和未知格式；不启用 ImageMagick 的全部格式作为上传入口。
- 保留 ABP 的 Stretch、Crop、Max、Min、Pad、BoxPad、Default、None 行为；裁剪和补边居中，透明补边。
- 自动应用 Exif 方向，输出方向归一，保留其他 Exif 与 ICC。控制器继续返回宽高和 Exif 信息。
- 动图按帧处理，保留 GIF 帧数、延迟和循环信息；TIFF 的独立页面尺寸不会按动画合并。
- 压缩后若体积更大，返回原数据及 ABP Canceled 状态。
- 现有 ImageHelper 保持公共方法签名和 PNG 输出，覆盖透明空图、圆角、九宫格合成。圆角保留原有透明度。
- 支持 CancellationToken；流输入有大小限制，调用方流保持打开，输出流从位置 0 开始。原生编解码调用期间不能即时打断取消，取消检查发生在调用前后和帧之间。

## 配置

ABP 图片适配器从 `Imaging:Magick` 绑定限制，默认输入上限 32 MiB、累计解码像素上限 40,000,000、帧数上限 128。先读取头信息检查，再解码并复检；输出尺寸同样受限制。多帧像素检查采用最大画布宽高乘帧数，可能保守拒绝大小差异很大的 TIFF。

```json
{
  "Imaging": {
    "Magick": {
      "MaxInputBytes": 33554432,
      "MaxDecodedPixels": 40000000,
      "MaxFrames": 128
    }
  }
}
```

Rctea.IM Host 保留原编码配置：JPEG 质量 60、WebP 质量 65、PNG 压缩等级 9。Host 的 Configure 覆盖 Commons 的质量默认值。静态 ImageHelper 沿用默认限制，不经过 DI 配置。

## 验证

- Commons Domain、Chat HttpApi 编译成功。
- Rctea.IM HttpApi.Host Release 构建成功，0 警告、0 错误。
- 新增 `ImageProcessing_Tests.cs`：36 个图片用例通过，包括六种格式、缩放模式、Exif 方向及元数据、GIF 动画、透明度、合成图、非法输入、截断数据、BigTIFF、资源限制、流生命周期与取消。
- 同批执行既有消息可见性、消息幂等领域用例，共 38 个用例通过。
- 另执行 12 个应用层消息权限/分页用例和 2 个 EF 消息可见性查询用例，全部通过；本次相关测试合计 52 个。
- Linux .NET 10 容器内实际执行 PNG/JPEG/WebP/GIF/BMP/TIFF 编码、解码和缩放，六种格式均通过。
- `dotnet nuget why` 确认 Chat HttpApi 与主 Host 不依赖 ImageSharp；最终 diff 检查通过。

本次未执行真实服务部署、数据库迁移或 NuGet 发布。

## 提交与部署注意

改动分布在 `IczpNet.AbpCommons`、`IczpNet.Chat`、`Rctea.IM` 三个仓库，需要一起提交。Chat 与 Rctea.IM 中全部 `IczpNet.AbpCommons.*` 引用统一为相邻仓库的源码项目引用，包括 Domain.Shared、Domain、Application.Contracts、Application、EntityFrameworkCore、MongoDB、HttpApi、HttpApi.Client、Installer 和 Host.Shared；Commons 内部同样使用项目引用。构建机必须保持现有目录布局并包含该仓库，不能混用同一系列的本地项目与 NuGet 包。

公开的 Commons 0.10.0 NuGet 包未被修改。若以后切换到 NuGet，必须先发布包含迁移实现的新版本，再将全部 Commons 引用一起切换并统一版本；恢复旧包会重新引入 ImageSharp。

Magick.NET 包携带原生运行库，发布时需保留对应目标平台的 runtime 文件。Linux smoke 检查已通过，但目标生产系统仍应使用自己的发布产物验证。编码器变化会改变图片字节、体积及部分像素取整结果，现有存储文件不需要迁移。

没有修改 Redis、消息分页、数据库结构、后台任务或 API 路由，不需要 Migration。图片处理仍是同步原生计算，高并发图片上传的 CPU/内存容量需要按实际部署规模评估。

## Commons 引用统一复核

将剩余 14 处有效 Commons NuGet 引用（Chat 9 处、Rctea.IM 5 处）转换为对应的本地项目引用，清理旧的重复注释。加上先前已迁移的 Domain，整个系列统一使用源码。

静态扫描三个仓库的全部 csproj，验证 41 条 Commons ProjectReference 目标存在，没有 Commons PackageReference 残留。实际还原结果确认 Chat HttpApi、Installer、MongoDB、HttpApi.Client、BackgroundWorker 及 Rctea.IM 五个宿主的 Commons 库类型全部为 project。

- Chat 解决方案 Release 编译成功；增量复核 0 警告、0 错误。
- Chat BackgroundWorker 单独 Release 编译成功，0 警告、0 错误。该项目不在 Chat 解决方案中。
- Rctea.IM 主 Host Release 编译成功，0 警告、0 错误。
- Rctea.IM 全解决方案未通过：Server 缺少 IczpNet.Pusher 引用，产生 34 个 CS0234/CS0246 错误；Web 菜单有一条已有 CS0162 警告。原 Commons Host.Shared NuGet 包与本地项目均只依赖 ABP MVC，不提供 Pusher。这些独立问题未在本次引用调整中修改。

参考：[Magick.NET 官方仓库](https://github.com/dlemstra/Magick.NET)、[14.17.2 NuGet 包](https://www.nuget.org/packages/Magick.NET-Q8-AnyCPU/14.17.2)。
