# 实验版 AssetBundle 近零成本部署说明

本文记录当前项目拆包后的资源托管方式。目标不是正式商业发行，而是让开发者和少量测试者可以安装 Android/iOS 包后，从网络下载资源运行游戏。

## 结论

优先使用 Cloudflare R2。

原因：

- R2 可以保存目录结构，能直接匹配 UTAGE 的服务器资源路径。
- 当前免费层足够实验使用：少量测试、约 3GB 资源、几个人下载基本接近 0 成本。
- R2 出网流量免费，适合放 AssetBundle。
- 比 GitHub Releases 更适合 Unity 运行时下载，因为 GitHub Releases 的资源文件是平铺的，不适合 UTAGE 默认的 `Android/Android`、`iOS/iOS` 目录式加载。

GitHub Releases 只作为备用方案：如果以后给项目加了“资源名 -> 完整 URL”的自定义下载映射，才建议用它。

## UTAGE 需要的服务器目录结构

当前 UTAGE 代码会按运行平台拼资源根路径。假设 `ServerUrl` 设置为：

```text
https://assets.example.com/theweepingswan/1.0.0
```

Android 运行时会访问：

```text
https://assets.example.com/theweepingswan/1.0.0/Android/Android
https://assets.example.com/theweepingswan/1.0.0/Android/<assetBundleName>
```

iOS 运行时会访问：

```text
https://assets.example.com/theweepingswan/1.0.0/iOS/iOS
https://assets.example.com/theweepingswan/1.0.0/iOS/<assetBundleName>
```

其中第一个 `Android` 或 `iOS` 是平台目录，第二个 `Android` 或 `iOS` 是 Unity AssetBundleManifest 文件名。不要改名。

建议最终上传结构：

```text
theweepingswan/
  1.0.0/
    Android/
      Android
      Android.manifest
      core.unity3d
      chapter_01.unity3d
      chapter_02.unity3d
      event_cg.unity3d
      character.unity3d
      bg.unity3d
      voice.unity3d
      bgm.unity3d
    iOS/
      iOS
      iOS.manifest
      core.unity3d
      chapter_01.unity3d
      chapter_02.unity3d
      event_cg.unity3d
      character.unity3d
      bg.unity3d
      voice.unity3d
      bgm.unity3d
```

实际文件名以 UTAGE/Unity 构建出的 AssetBundle 为准。上面的名字只是推荐分组。

## R2 部署步骤

1. 注册或登录 Cloudflare。

2. 创建 R2 bucket。

   推荐 bucket 名：

   ```text
   theweepingswan-assets
   ```

3. 开启公开访问。

   实验阶段可以先用 Cloudflare 提供的公开访问地址。正式或长期测试建议绑定自定义域名，例如：

   ```text
   https://assets.example.com
   ```

4. 上传 AssetBundle。

   如果使用网页后台，按目录逐级上传：

   ```text
   theweepingswan/1.0.0/Android/*
   theweepingswan/1.0.0/iOS/*
   ```

   如果使用命令行，可以安装 Wrangler：

   ```powershell
   npm install -g wrangler
   wrangler login
   ```

   上传单个平台目录：

   ```powershell
   wrangler r2 object put theweepingswan-assets/theweepingswan/1.0.0/Android/Android --file .\BuildAssetBundles\Android\Android
   wrangler r2 object put theweepingswan-assets/theweepingswan/1.0.0/Android/core.unity3d --file .\BuildAssetBundles\Android\core.unity3d
   ```

   文件多时可以用脚本循环上传，但必须保持对象 key 和本地目录结构一致。

   本工程已经提供脚本：

   ```powershell
   .\Tools\Deployment\Prepare-AssetBundleDeploy.ps1 -SourceRoot .\BuildAssetBundles -Version 1.0.0 -Clean
   .\Tools\Deployment\Upload-R2.ps1 -Bucket theweepingswan-assets -LocalRoot .\Deploy\AssetServer
   ```

   其中 `BuildAssetBundles` 是 Unity/UTAGE 输出 AssetBundle 的目录，里面应包含：

   ```text
   BuildAssetBundles/
     Android/
       Android
       ...
     iOS/
       iOS
       ...
   ```

5. 在 Unity/UTAGE 项目里配置资源地址。

   ServerUrl 设置为版本目录，不要包含平台名：

   ```text
   https://assets.example.com/theweepingswan/1.0.0
   ```

   不要写成：

   ```text
   https://assets.example.com/theweepingswan/1.0.0/Android
   ```

   因为 UTAGE 会自己追加 `Android` 或 `iOS`。

6. 测试 URL。

   浏览器能直接下载或访问这些文件才算成功：

   ```text
   https://assets.example.com/theweepingswan/1.0.0/Android/Android
   https://assets.example.com/theweepingswan/1.0.0/iOS/iOS
   ```

   如果这两个地址 404，Unity 客户端也会下载失败。

## 本地零成本测试方案

只给自己手机测试时，也可以不用云存储，直接在电脑上开本地 HTTP 服务。

目录示例：

```text
E:\AssetServer\theweepingswan\1.0.0\Android\Android
E:\AssetServer\theweepingswan\1.0.0\Android\core.unity3d
```

启动本地服务器：

```powershell
cd E:\AssetServer
python -m http.server 8000
```

本工程也提供脚本启动本地静态服务器：

```powershell
.\Tools\Deployment\Start-LocalAssetServer.ps1 -Port 8000
```

停止服务器：

```powershell
.\Tools\Deployment\Stop-LocalAssetServer.ps1
```

手机和电脑在同一 Wi-Fi 下时，ServerUrl 设置为：

```text
http://<电脑局域网IP>:8000/theweepingswan/1.0.0
```

注意：

- Android 新版本可能限制明文 HTTP。若下载失败，改用 HTTPS 隧道工具，或在 Android 网络安全配置中允许测试 HTTP。
- 电脑必须开机，手机必须能访问这台电脑。
- 这个方案不适合给外部测试者长期使用。

## GitHub Releases 备用方案

GitHub Releases 接近 0 成本，适合手动下载资源包或少量公开测试，但不建议直接给当前 UTAGE 默认下载逻辑使用。

原因：

- GitHub Release assets 是平铺文件，不是真正的目录托管。
- 当前 UTAGE 会请求 `.../Android/Android` 和 `.../iOS/iOS` 这种目录 URL。
- 如果不改下载代码，GitHub Releases 的 URL 结构很难完全匹配。

可以使用 GitHub Releases 的情况：

- 把 AssetBundle 打成 zip，让测试者手动下载。
- 或者项目内新增一个自定义 manifest，里面记录每个资源的完整下载 URL。
- 或者以后改 UTAGE 的 AssetBundle URL 生成逻辑，让它能访问 GitHub Release asset 的完整 URL。

GitHub Release URL 示例：

```text
https://github.com/<owner>/<repo>/releases/download/assets-v1.0.0/Android_core.unity3d
```

如果用这种方式，文件命名建议带平台前缀：

```text
Android_Android
Android_core.unity3d
Android_chapter_01.unity3d
iOS_iOS
iOS_core.unity3d
iOS_chapter_01.unity3d
```

但这需要客户端额外做 URL 映射，不能直接套用当前 UTAGE 默认配置。

## 版本更新规则

每次资源更新都新建一个版本目录：

```text
theweepingswan/1.0.0/
theweepingswan/1.0.1/
theweepingswan/1.1.0/
```

客户端配置只改 ServerUrl：

```text
https://assets.example.com/theweepingswan/1.0.1
```

不要覆盖旧目录，除非确认所有测试包都不再使用旧版本资源。

## 分包建议

当前项目大资源主要在：

- `Assets/Resources/starveling/texture/event`：事件图，约 1.37GB。
- `Assets/Resources/starveling/texture/character`：立绘，约 549MB。
- `Assets/Resources/starveling/texture/bg`：背景，约 362MB。
- `Assets/Resources/starveling/sound`：声音，约 396MB。
- `Assets/Utage/Sample/Resources`：示例资源，约 305MB，正式包应排除。

推荐拆包顺序：

1. `core`：标题、公共 UI、必要字体、小音效、首段可玩内容。
2. `chapter_01`、`chapter_02`：按章节拆剧情资源。
3. `event_cg`：大事件图。
4. `character`：角色立绘。
5. `bg`：背景。
6. `voice`：语音。
7. `bgm`：音乐。
8. `gallery_highres`：图库高清资源，需要时再下载。

## 交接检查表

部署者需要确认：

- Android 和 iOS AssetBundle 是分别构建的，不能混用。
- R2 上存在 `Android/Android` 和 `iOS/iOS` manifest 文件。
- ServerUrl 只写到版本目录，不包含平台名。
- 浏览器能访问 manifest URL。
- 手机首次运行时能下载资源。
- 更新资源时使用新版本目录，不直接覆盖旧版本。
- 不把 `Assets/Resources` 里的 2GB 以上大资源继续打进主包。

## 参考链接

- Cloudflare R2 pricing: https://developers.cloudflare.com/r2/pricing/
- Cloudflare R2 public buckets: https://developers.cloudflare.com/r2/data-access/public-buckets/
- GitHub Releases: https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases
- GitHub Pages limits: https://docs.github.com/en/pages/getting-started-with-github-pages/about-github-pages
