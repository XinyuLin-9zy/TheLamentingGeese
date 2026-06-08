# The Lamenting Geese

> Unity + UTAGE visual novel reconstruction project for **The Weeping Swan / 哀鸿**.

本仓库整理了一个可在 Unity Editor、本地静态服务器、Android、iOS 之间切换运行的 UTAGE 视觉小说工程。项目默认不绑定任何公开资源服务器；接收者需要自行构建 AssetBundle，并配置自己的本地服务器、CDN、对象存储或静态托管地址。

---

## 免责声明与侵权声明

本项目仅用于技术研究、学习交流、工程迁移、运行时兼容性验证与个人备份整理，不用于商业发行，也不代表对原作内容、素材、商标、音乐、字体、脚本或其他受保护资产拥有任何权利。

仓库中如存在来自原作或第三方的内容，其版权、商标权及相关权益均归原权利人所有。本项目维护者不主张对这些内容的所有权，也不鼓励任何未经授权的传播、分发、二次销售或商业使用。

如果你是相关权利人，认为本仓库中的任何内容侵犯了你的合法权益，请通过仓库 Issue 或仓库页面提供的联系方式提出删除、替换或下架请求。维护者会在核实后尽快处理。

使用、分享、构建或部署本工程时，请自行确认你所在地区的法律要求，并确保只在你拥有合法授权的范围内使用相关资源。

---

## 项目概览

| 项目 | 内容 |
| --- | --- |
| Unity 版本 | `2022.3.62f1` |
| 视觉小说框架 | UTAGE 4 |
| 渲染管线 | Universal Render Pipeline |
| 主场景 | `Assets/Scenes/level0.unity` |
| Unity Product Name | `TheLamentingGeese` |
| 默认 Game ID | `theweepingswan` |
| 默认资源版本 | `1.0.0` |
| Editor 默认资源模式 | 本地资源模式 |
| Android / iOS 默认资源模式 | 远程 AssetBundle 模式 |

当前工程已经补充：

- Android / iOS 运行所需的远程 AssetBundle 加载配置。
- 移动端构建菜单与默认构建输出路径。
- 本地静态资源服务器脚本。
- AssetBundle 部署目录整理脚本。
- 可选 Cloudflare R2 上传脚本。
- 面向分享工程的配置说明与隐私注意事项。

## 推荐阅读顺序

1. 第一次打开项目：先看 [快速开始](#快速开始)。
2. 要跑移动端包：看 [AssetBundle 构建与部署](#assetbundle-构建与部署) 和 [Android--ios-构建](#android--ios-构建)。
3. 要接自己的服务器：看 [资源服务器地址配置](#资源服务器地址配置)。
4. 要把工程分享给别人：看 [分享工程前检查](#分享工程前检查)。

## 快速开始

```bash
git clone <repo-url>
```

然后：

1. 用 Unity Hub 选择 Unity `2022.3.62f1` 打开仓库根目录。
2. 等待 Unity 完成 Package restore 和 Asset import。
3. 打开 `Assets/Scenes/level0.unity`。
4. 在 Unity 菜单选择：

```text
Codex/Asset Runtime Mode/Editor Local Assets
```

5. 点击 Play。Editor 中默认使用本地资源，不需要启动 AssetBundle 服务器。

如果要测试 Android / iOS Player，需要先构建 AssetBundle、整理部署目录，并启动本地资源服务器或配置自己的远程静态托管。

## 目录结构

```text
Assets/
  Editor/                          自定义构建、AssetBundle、iOS 图标等编辑器脚本
  RemoteAssets/starveling/          会被打入远程 AssetBundle 的 UTAGE 资源
  Resources/                        运行时配置、本地 UI 资源、字体、prefab 等
  Scenes/level0.unity               主场景
  Scripts/Assembly-CSharp/          项目运行时代码和 UTAGE 补丁胶水代码
  Utage/                            UTAGE 框架源码、模板和示例资源
  Spine/                            Spine Runtime / Editor 集成
  AppIcon/                          Android / iOS 图标资源

Tools/Deployment/                   本地静态服务器、部署布局、R2 上传脚本
Deploy/README.md                    部署工作区的简短说明
ProjectSettings/                    Unity 工程设置
Packages/                           Unity Package Manager manifest / lock 文件

BuildAssetBundles/                  AssetBundle 构建输出，Git 忽略
Builds/                             Android / iOS 构建输出，Git 忽略
Deploy/AssetServer/                 可直接作为静态服务器根目录的资源布局，Git 忽略
Library/, Temp/, Logs/              Unity 自动生成目录，Git 忽略
```

## 环境要求

| 环境 | 用途 |
| --- | --- |
| Unity Editor `2022.3.62f1` | 打开、编辑、构建工程 |
| Android Build Support | 构建 Android APK 或 Android AssetBundle |
| iOS Build Support | 构建 iOS Xcode 工程或 iOS AssetBundle |
| Xcode | 在 macOS 上继续编译 iOS App |
| Python 3 | 启动本地静态资源服务器 |
| Node.js + Wrangler | 上传到 Cloudflare R2 时才需要 |

Unity Package 依赖写在 `Packages/manifest.json`，首次打开工程时 Unity 会自动恢复。

## 运行时资源模式

工程增加了一层 `CodexAssetRuntimeMode` 配置，用来控制 UTAGE 加载本地资源还是远程 AssetBundle。

### 菜单入口

```text
Codex/Asset Runtime Mode/Editor Local Assets
Codex/Asset Runtime Mode/Remote AssetBundles
Codex/Asset Runtime Mode/Remote Target/Android
Codex/Asset Runtime Mode/Remote Target/iOS
Codex/Asset Runtime Mode/Remote Target/Use Active Build Target
Codex/Asset Runtime Mode/Print Current Mode
```

### 模式说明

| 模式 | 说明 |
| --- | --- |
| `Editor Local Assets` | 编辑器内使用本地 UTAGE 资源，适合剧情、UI、脚本调试 |
| `Remote AssetBundles` | 通过 UTAGE 的 AssetBundle 下载逻辑加载远程资源 |
| Player 构建默认值 | Android / iOS 默认使用 `Remote AssetBundles` |

### 命令行覆盖

覆盖运行模式：

```bash
-assetRuntimeMode local
-assetRuntimeMode remote
```

覆盖资源服务器地址：

```bash
-assetServerUrl https://assets.example.com/theweepingswan/1.0.0
```

`ServerUrl` 必须指向版本根目录，不要带平台目录。UTAGE 会自行追加 `Android`、`iOS`、`Windows` 或 `OSX`。

正确：

```text
https://assets.example.com/theweepingswan/1.0.0
```

错误：

```text
https://assets.example.com/theweepingswan/1.0.0/iOS
```

## 资源服务器地址配置

默认配置文件在：

```text
Assets/Resources/CodexAssetBundleServerUrl_Android.txt
Assets/Resources/CodexAssetBundleServerUrl_iOS.txt
Assets/Resources/CodexAssetBundleServerUrl_Windows.txt
Assets/Resources/CodexAssetBundleServerUrl_OSX.txt
```

共享工程中的默认值应保持为本地开发地址：

```text
Android: http://10.0.2.2:8000/theweepingswan/1.0.0
iOS:     http://127.0.0.1:8000/theweepingswan/1.0.0
```

常见覆盖方式：

```bash
CODEX_ASSET_SERVER_URL=https://assets.example.com/theweepingswan/1.0.0
CODEX_ASSET_SERVER_URL_ANDROID=https://assets.example.com/theweepingswan/1.0.0
CODEX_ASSET_SERVER_URL_IOS=https://assets.example.com/theweepingswan/1.0.0
```

也可以创建本地覆盖文件：

```text
Deploy/asset-server-url-Android.txt
Deploy/asset-server-url-iOS.txt
```

这些文件已被 Git 忽略，适合写私有服务器地址、临时测试地址或个人对象存储地址。

## AssetBundle 构建与部署

移动端 Player 默认从远程 AssetBundle 读取资源。完整流程是：

```text
构建 AssetBundle
  -> 整理静态服务器目录
  -> 启动本地服务器或上传到静态托管
  -> 配置 ServerUrl
  -> 构建 Android / iOS Player
```

### Unity 菜单

AssetBundle 菜单在：

```text
Codex/AssetBundles/
```

常用菜单：

```text
Prepare Remote Resource Layout
Build Android and iOS
Build iOS
Build Editor Host
Prepare Layout and Build Android and iOS
Prepare Layout and Build Editor Host
```

构建输出目录：

```text
BuildAssetBundles/<Platform>/
```

关键 manifest 文件示例：

```text
BuildAssetBundles/Android/Android
BuildAssetBundles/Android/Android.manifest
BuildAssetBundles/iOS/iOS
BuildAssetBundles/iOS/iOS.manifest
```

不要改平台 manifest 文件名。UTAGE 会请求类似：

```text
<ServerUrl>/Android/Android
<ServerUrl>/iOS/iOS
```

### 准备静态服务器目录

构建完 AssetBundle 后，需要整理成静态服务器目录结构：

```text
Deploy/AssetServer/
  theweepingswan/
    1.0.0/
      Android/
        Android
        Android.manifest
        ...
      iOS/
        iOS
        iOS.manifest
        ...
```

macOS / Linux：

```bash
Tools/Deployment/prepare-asset-bundle-deploy.sh \
  --source-root BuildAssetBundles \
  --version 1.0.0 \
  --clean
```

Windows PowerShell：

```powershell
.\Tools\Deployment\Prepare-AssetBundleDeploy.ps1 `
  -SourceRoot .\BuildAssetBundles `
  -Version 1.0.0 `
  -Clean
```

只准备一个平台：

```bash
Tools/Deployment/prepare-asset-bundle-deploy.sh --platform iOS --clean
Tools/Deployment/prepare-asset-bundle-deploy.sh --platform Android --clean
```

## 本地资源服务器

macOS / Linux：

```bash
Tools/Deployment/start-local-asset-server.sh --port 8000
```

前台运行：

```bash
Tools/Deployment/start-local-asset-server.sh --port 8000 --foreground
```

停止：

```bash
Tools/Deployment/stop-local-asset-server.sh
```

Windows PowerShell：

```powershell
.\Tools\Deployment\Start-LocalAssetServer.ps1 -Port 8000
.\Tools\Deployment\Stop-LocalAssetServer.ps1
```

URL 示例：

```text
编辑器或 iOS 模拟器：
http://127.0.0.1:8000/theweepingswan/1.0.0

Android 模拟器：
http://10.0.2.2:8000/theweepingswan/1.0.0

同一局域网里的真机：
http://<电脑局域网IP>:8000/theweepingswan/1.0.0
```

移动端开发构建当前允许 HTTP 明文请求，方便本地测试。公开测试或长期分发建议使用 HTTPS。

## Cloudflare R2 或其他静态托管

任何能保留路径结构并直接返回文件的静态托管都可以使用，例如：

- Cloudflare R2 + Public Bucket 或自定义域名。
- S3 兼容对象存储。
- CDN 静态文件源站。
- 自己的 Nginx / Caddy / Apache 静态服务器。

示例版本根 URL：

```text
https://assets.example.com/theweepingswan/1.0.0
```

上传完成后至少测试：

```text
https://assets.example.com/theweepingswan/1.0.0/Android/Android
https://assets.example.com/theweepingswan/1.0.0/iOS/iOS
```

如果这两个 URL 是 404，客户端也会下载失败。

R2 上传示例：

```bash
npm install -g wrangler
wrangler login
Tools/Deployment/upload-r2.sh --bucket YOUR_BUCKET_NAME --local-root Deploy/AssetServer
```

Windows PowerShell：

```powershell
npm install -g wrangler
wrangler login
.\Tools\Deployment\Upload-R2.ps1 -Bucket YOUR_BUCKET_NAME -LocalRoot .\Deploy\AssetServer
```

脚本使用 `wrangler r2 object put ... --remote`，会上传到真实 R2 bucket，而不是 Wrangler 本地开发存储。

不要把个人 R2 地址、bucket 名、token、`.wrangler/` 或 Cloudflare 登录缓存提交到仓库。

## Android / iOS 构建

### Android

菜单：

```text
Codex/Build Android APK
```

默认输出：

```text
Builds/Android/TheLamentingGeese-1.3-android-arm64.apk
```

构建脚本会：

- 切换到 Android BuildTarget。
- 确保远程 AssetBundle 运行配置存在。
- 使用 IL2CPP。
- 使用 ARM64。
- 打开 Internet 权限。
- 开发构建配置允许 HTTP 资源服务器。

可用环境变量改输出路径：

```bash
CODEX_ANDROID_OUTPUT=Builds/Android/MyBuild.apk
```

### iOS

菜单：

```text
Codex/Build iOS Xcode Project
```

默认输出：

```text
Builds/iOS/TheLamentingGeese-iOS
```

构建脚本会：

- 切换到 iOS BuildTarget。
- 确保远程 AssetBundle 运行配置存在。
- 使用 IL2CPP。
- 设置 Device SDK。
- 应用 iOS App Icon。
- 导出后补齐 Xcode AppIcon 配置。
- 开发构建配置允许 HTTP 资源服务器。

可用环境变量改输出路径：

```bash
CODEX_IOS_OUTPUT=Builds/iOS/MyXcodeProject
```

## 常用验证

如果 `.csproj` 已生成，可以运行：

```bash
dotnet build Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly
dotnet build Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly
```

检查资源服务器是否可访问：

```bash
curl -I http://127.0.0.1:8000/theweepingswan/1.0.0/iOS/iOS
curl -I http://127.0.0.1:8000/theweepingswan/1.0.0/Android/Android
```

检查是否误提交私有部署输出：

```bash
git status --ignored --short BuildAssetBundles Deploy/AssetServer Builds
```

检查待提交 diff 中是否含私有 URL：

```bash
git diff -- Assets/Resources Tools/Deployment README.md Deploy/README.md
```

## 分享工程前检查

分享或提交前请确认：

- `.wrangler/` 没有被提交。
- Cloudflare token、R2 公开地址、bucket 名、个人账号信息没有被提交。
- `BuildAssetBundles/`、`Builds/`、`Deploy/AssetServer/` 没有被提交。
- `Assets/Resources/CodexAssetBundleServerUrl_<Platform>.txt` 保持本地默认或通用占位地址。
- 个人测试地址写在 `Deploy/asset-server-url-*.txt` 或环境变量里。
- 接收者知道需要自己构建并托管 AssetBundle。

## 故障排查

| 现象 | 优先检查 |
| --- | --- |
| Editor 可以运行，手机黑屏或卡加载 | Player 是否处于远程 AssetBundle 模式，资源服务器是否可访问 |
| Android 模拟器连不上本机服务器 | Android 模拟器应使用 `http://10.0.2.2:8000/...` |
| iOS 模拟器连不上本机服务器 | iOS 模拟器可使用 `http://127.0.0.1:8000/...` |
| 真机连不上本机服务器 | 使用电脑局域网 IP，并确认防火墙、端口、同网段 |
| manifest 404 | 确认 `<ServerUrl>/<Platform>/<Platform>` 文件存在 |
| R2 上传后客户端失败 | 确认对象路径完整、公开访问权限、Content-Type 不阻断下载 |

## 补充文档

- `Deploy/README.md`：部署工作区简明说明。
- `ASSET_BUNDLE_ZERO_COST_DEPLOYMENT.md`：AssetBundle 静态托管和 R2 部署讨论。
- `ARCHITECTURE.md`：更细的 UTAGE / MCP / 项目架构记录。
- `PROJECT_STATUS.md`：历史修复记录和验证笔记。
