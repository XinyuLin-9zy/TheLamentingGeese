# 哀鸿 (The Weeping Swan) - Unity 项目架构与当前状态

> 协作参考文档 — 描述项目现状、架构和待修复问题。保持同步更新。

## 项目概述

基于 **UTAGE4 v4.2.7** 视觉小说引擎的 Unity 2022.3.62f2c1 项目。中国历史题材视觉小说，内部代号 "Starveling"。

### 技术栈
- Unity 2022.3.62f2c1 LTS
- UTAGE4 (914 .cs 完整源码)
- Odin Inspector 3.3.1.10（轻量版，仅序列化功能）
- Spine 2D (spine-unity) — Title 背景动画
- DOTween, UniTask, Febucci TextAnimator, RubyTextMeshPro
- PureMVC — 游戏逻辑架构
- AccessibleButton/UAP — 无障碍支持

### 目录结构
```
Assets/
├── Utage/Scripts/     # UTAGE4 引擎完整源码 (914 .cs)
├── Scripts/Assembly-CSharp/  # 游戏特定代码 (~150 .cs，部分为 stub)
├── Plugins/           # 第三方 DLL (21个)
├── Resources/starveling/     # 游戏资源
│   ├── sound/         # 音频 (BGM 74, SE 376, Voice 8536)
│   └── texture/       # 纹理 (BG 368, Character 794, Event 920)
├── MonoBehaviour/     # 剧情数据
│   ├── Starveling.book.asset      # 完整剧本 (693K 行 YAML)
│   ├── Boot.chapter.asset         # 章节配置
│   ├── Starveling.scenarios.asset # 场景列表
│   ├── 1_SkeletonData.asset       # Spine 骨骼数据
│   └── 1_Atlas.asset              # Spine 图集
├── GameObject/        # 预制体
│   └── UI_ChapterTitle.prefab     # 章节标题预制体（RectMask2D GUID 已修复）
├── Scenes/level0.unity    # 主场景 (1937 GameObject, 184K行)
├── Font/              # 中英文字体 (.otf/.ttf)
└── TextAsset/1.json   # Spine 骨骼 JSON
```

## UTAGE4 架构

### 启动流程
```
场景加载
  │
  ├─ AdvEngineStarter.Awake()
  │   └─ LoadEngineAsync()
  │       ├─ AssetFileManager.InitLoadTypeSetting(Local)
  │       └─ Engine.BootFromExportData(scenarios, "Starveling")
  │           └─ CoBootInit() → 初始化所有子系统
  │
  └─ UtageUguiBoot.Start()
      └─ CoUpdate() → Open() → 等待 IsWaitBootLoading → title.Open()
```

### 剧本数据流
```
AdvEngineStarter.scenarios
  → Starveling.scenarios.asset
    → chapters[0] → Boot.chapter.asset
      → dataList[0] → Starveling.book.asset (693K行剧本)
```

### 命令解析
- 标准命令 (Text, Bg, Character, Selection, Bgm 等) — `AdvCommandParser.CreateCommandDefault()` switch
- 自定义命令 — `AdvCommandParser.OnCreateCustomCommandFromID` 事件
- `CustomCommander.OnBootInit()` 注册了所有游戏特有命令 (UnlockPlotMap, SetChapterName, ChapterAnimation, ScreenText, AddSteamAchiKey, AutoSave 等)

### 文本显示链
```
AdvEngine → UiManager → MessageWindowManager
  └─ MessageWindow (AdvUguiMessageWindow)
      ├─ text → MessageText (1) → NovelTextForTextAnimator (extends UguiNovelText → Text)
      └─ nameText → NameText → NovelTextForTextAnimator
```
`NovelTextForTextAnimator` 是游戏特定的文本渲染类，继承自 `UguiNovelText` → `UnityEngine.UI.Text`。

---

## 当前修复状态 (2026-05-12)

以下记录只描述当前项目状态，后续排查前应优先回看并更新本段。

### 主界面右下角按钮
- 状态：已做运行时绑定兜底。
- 涉及：`UtageUguiMainGame.cs`
- 说明：`Save/Load/Config/Auto/Skip/History/HideUI/QSave/QLoad/PlotMap` 不再完全依赖场景 Inspector 里的 OnClick/OnValueChanged 引用。

### 选项窗口
- 状态：已做运行时初始化重试和空引用保护，仍建议在真实剧情分支处 Play 验证。
- 涉及：`AdvUguiSelectionManager.cs`、`AdvUguiSelection.cs`
- 说明：解决 SelectionManager/Engine/ListView/Prefab 引用恢复不及时导致选项不显示的风险。

### 货币 UI
- 状态：已按需求改为左下角按需显示。
- 涉及：`UI_DialogMsg.cs`、`MoneyControlCommand.cs`、`AdvCommandGetMoney.cs`、`AdvCommandUseMoney.cs`、`AdvCommandRemianMoney.cs`
- 说明：货币栏由 `IsMoney` 参数控制显示/隐藏，读取 `money` 参数刷新数值，不应常驻左上角。

### ChapterAnimation / 动态背景
- 状态：已改为从 `Resources/title/...` 加载并实例化 `UI_ChapterTitle` 播放。
- 涉及：`TitleAnimationCommand.cs`、`CustomCommander.cs`、`UI_TitleAnimation.cs`
- 说明：用于修复章节标题/主界面动态背景资源引用丢失导致不显示的问题。

### 存档/读取界面
- 状态：已修复打开后不可操作的问题，并通过 MCP Play 复测。
- 涉及：`UtageUguiSaveLoad.cs`、`UtageUguiSaveLoadItem.cs`、`UguiGridPage.cs`、`Assets/GameObject/SaveLoadItem.prefab`
- 根因：
  - `level0.unity` 中 `SaveLoad/GridPage.grid` 为 `{fileID: 0}`，列表创建可能空引用中断。
  - SaveLoadItem prefab 的按钮 OnClick 为空，且存在缺失脚本 GUID，槽位点击无法进入保存/读取逻辑。
  - 返回/翻页按钮也存在 Inspector 事件或引用丢失风险。
- 当前处理：
  - `UtageUguiSaveLoad` 运行时查找 `GridPage/mainGame/saveRoot/loadRoot`，自动绑定返回按钮和每个存档槽点击。
  - `UtageUguiSaveLoad` 在 `guideMessage` 丢失时运行时创建 `__RuntimeGuideMessage`，并对缺失的 `SystemText` 本地化项使用安全 fallback，保证空槽读取/自动存档不可覆盖有可见提示且不打 Error。
  - `UtageUguiSaveLoadItem` 运行时补 `Button`，自动查找文本、日期、截图组件，并保护空截图引用。
  - `UguiGridPage` 运行时恢复/创建 `GridLayoutGroup`，绑定翻页按钮，并防止每页数量为 0。
  - `Assets/GameObject/SaveLoadItem.prefab` 的缺失脚本 GUID 已改为当前存在的 `UtageUguiSaveLoadItem` GUID。
- 验证：
  - `dotnet build .\Utage.csproj --no-restore -v:quiet`
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet`
  - 两者顺序执行通过。并行执行可能因 `obj\Debug\Utage.dll` 文件锁产生假失败。
  - MCP Play 复测：Title 打开 Load 成功，5 个槽位/5 个 Button；空槽点击出现运行时提示且 Console 0 error；saved slot 点击可进入 MainGame；MainGame 打开 Save 成功，5 个槽位/5 个 Button。

## 当前修复状态 (2026-05-13)

本节点按“待修复问题”做了静态收敛，主要结论如下：

- 中文字体：`Assets/GameObject`、`Assets/Resources`、`Assets/Scenes` 中的内置 Arial `m_Font` 残留已清零。本次补改 `Assets/Resources/Accessibility Manager.prefab`、`Assets/GameObject/DebugLogItem.prefab`、`Assets/Resources/UAP Virtual Keyboard.prefab`，统一指向 `SourceHanSerifCN-Regular.otf`。
- 脚本 GUID：问题 2b 表内 10 个旧 GUID 在项目主资源目录无残留；`Assets/GameObject`、`Assets/Resources`、`Assets/Scenes` 中也未发现 `m_Script: {fileID: 0}`。
- UI 空引用：`Assets/GameObject`、`Assets/Resources`、`Assets/Scenes` 中未发现 `m_Content: {fileID: 0}`；`UguiListView` 和 `UguiGridPage` 的运行时兜底仍保留。
- `UI_ChapterTitle.prefab`：root 的 `RectMask2D` GUID 已修复为当前 Unity UI 包的 `3312d7739989d2b4e91e6319e9a96d76`，Unity 加载后 `LayoutGroups=0`；场景 `titleAnimationPrefab` 仍正确指向该 prefab。
- Title Spine：场景 Canvas 的 `m_AdditionalShaderChannelsFlag` 为 25；`SkeletonGraphic.skeletonDataAsset` 指向 `1_SkeletonData.asset`，`startingAnimation` 为 `idle`。`UtageUguiTitle.EnsureSpineTitleBackground()` 会在打开 Title 时强制激活并初始化 Spine。
- 验证：
  - `dotnet build .\Utage.csproj --no-restore -v:quiet`：通过。
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet`：通过。
  - 静态检查无 `m_Script fileID:0`、无 `m_Content fileID:0`、无项目主资源内置 Arial `m_Font` 残留。
- 限制：本节点从 shell 访问 Unity MCP 旧 GET 端点返回 HTTP 400，因此未完成新的 Play 模式复测。Save/Load、真实剧情 Selection 分支、Title Spine 仍建议在 Unity Play 中做最终人工/桥接验证。

## 当前修复状态 (2026-05-13 MCP Play 复测)

本节点确认 Unity MCP 正确入口为 `POST http://127.0.0.1:7891/api/...`；`GET /api/context` 属于包内线程问题，不作为验证入口。

- Unity MCP `/api/compilation/errors`：0 errors。
- Play 启动：`AdvEngine.IsWaitBootLoading == false`，Title active。
- Title Spine：`Canvas-AdvUI/Title/BG/Spine` active，`SkeletonGraphic.skeletonDataAsset == 1_SkeletonData`，当前动画 `idle`，`Skeleton != null`；Console 无 Spine material/atlas/animation error。
- Save/Load：
  - Title `OnTapLoad()` 可打开 Load，`UguiGridPage` 创建 5 个 active slot，5 个 slot 都有 `Button`。
  - 当前测试环境发现 2 个 saved slot；点击 saved slot 可进入 MainGame，场景标签到 `map_ji_1`，Console 无 error。
  - MainGame `OnTapSave()` 可打开 Save，5 个 active slot，5 个 slot 都有 `Button`。
  - 空槽 Load 原本没有可见提示，因为场景 `guideMessage` 为 null 且 `SystemText` 缺少对应本地化项；已在 `UtageUguiSaveLoad` 中新增运行时 `__RuntimeGuideMessage` fallback 和安全本地化 fallback。复测空槽点击后提示条 active/playing，Console 无 error。
- Selection：
  - `AdvUguiSelection` 原本在文本引用丢失时动态添加 `UguiNovelText`，会触发 `UguiNovelTextGenerator` 空引用；已改为动态添加普通 `UnityEngine.UI.Text` 并使用中文 OS 字体 fallback。
  - MCP smoke test 注入 1 个选项后，Selection UI active、创建 1 个 item、Button 存在、点击后 `SelectionManager.TotalCount == 0` 且 `IsWaitInput == false`，Console 无 error。
  - 真实剧情分支仍建议人工走一遍视觉确认，但当前创建/渲染/点击链已通过运行态烟测。
- Layout：上述 Play 路径 Console 未出现 `LayoutRebuilder.PerformLayoutCalculation` error。
- 仍存在 4 条 `TextAnimator_TMP` Required Components warning（NameText/MessageText），本轮未处理；它们不是当前文档问题中的阻断错误。

## 当前修复状态 (2026-05-13 Title/Gallery/PlotMap 复测)

本节点处理主页 `Gallery`/`Archive`/`PlotMap`/`ExtraStory` 一组按钮与 `UI_PlotMap` 文本、横向滚动条问题。

- Title 按钮：
  - `UtageUguiTitle` 运行时补绑 `Archive`、`PlotMap`、`ExtraStory`、`Exit`。
  - `AccessibleButton` 不再把 `Archive`/`PlotMap`/`ExtraStory` 误路由到 Gallery，而是分发到对应 Title 方法。
  - `PlotMap` 打开后通过反射调用单参数 `ShowMap(bool)`，避开 Unity `SendMessage` 对可选参数方法的匹配错误。
- Gallery：
  - `UtageUguiGallery` 会从 `TabButtons` 直接子对象名恢复 `CgGallery`、`SoundRoom`、`VoiceCollection` view 映射。
  - 顶部页签对象不是标准 `Toggle/Button` 时，运行时补 `EventTrigger` click/submit 绑定；`VoiceCollection` 作为非 `UguiView` 对象也能正常 active。
  - `OpenNamedView(this, "SceneGallery")` 用于 Archive；打开 SceneGallery 前会关闭 Cg/Sound/Voice 等运行态 view，避免互相叠加。
- `UI_PlotMap`：
  - 运行时恢复 `Scroll View`/`Viewport`/`Content`/`Scrollbar Horizontal`/`ScrollRect` 引用。
  - `Scrollbar Horizontal` 绑定 handle 与 content，复测 `bar.value 0 -> 1` 时 content x 从 `0` 到 `-12080`。
  - Content 下残留的伪 `Arrow` Scrollbar 会被禁用，避免抢拖拽事件。
  - 章节节点原本只有 `Lock/Image/Text`，解锁态关闭 Lock 后节点名不可见；`UI_PlotChapterElement` 现在会在解锁态补 `__RuntimeChapterLabel`，使用现有中文字体并按节点 Rect 拉伸。
  - `RefreshUI()` 会重复刷新章节锁定状态，不再因 `Init()` 只执行一次导致 Unlock/Reset 后 UI 不更新。
- `UguiGridPage`：
  - 如果目标对象已有其他 `LayoutGroup`，不再强行在同一对象添加 `GridLayoutGroup`，改建 `__RuntimeGrid`，修复打开 `VoiceCollection` 时的 `InitializeAddedGrid` 空引用。
- MCP Play 复测：
  - `AdvEngine.IsWaitBootLoading == false`，Title active。
  - Gallery 打开成功；触发 `VoiceCollection` 页签 EventTrigger 后 `voice == active` 且 `CgGallery/SoundRoom == inactive`。
  - Archive 打开 `SceneGallery` 成功且 `VoiceCollection` 已关闭。
  - PlotMap 打开成功，65 个运行时章节标签可见；`Text_Process` 显示“完成度”，进度值显示 `100%`。
  - `Scrollbar Horizontal` handle 存在，拖动值可驱动 Content；残留 `Arrow` Scrollbar disabled。
  - Unity MCP `/api/console/log` error count：0。

### Title Spine 视觉/材质修复 (2026-05-14)

本节点接手修复 Title Spine 视觉不对的问题，当前确认有三层问题：Title 运行时布局兜底导致缩放/裁切异常；`1_SkeletonData` 资产预览链路中的 atlas 材质误用了导出器伪 shader；游戏场景中的 `SkeletonGraphicDefault` UI 材质也误用了导出器伪 shader。

- 根因 1：运行时缩放/裁切
  - `UtageUguiTitle.EnsureSpineTitleBackground()` 原本把 `Canvas-AdvUI/Title/BG/Spine` 拉满父节点后又强制 `localScale = (5,5,5)`。
  - 原代码还把 `MatchRectTransformWithBounds` 当成属性反射写入，但 Spine runtime 中它是方法，因此没有真正按 mesh bounds 重新建立 RectTransform/referenceSize。
  - MCP Play 读取到修复前 mesh bounds 为 `9230 x 5224`、父容器为 `1920 x 1080`，叠加强制 scale 后极易出现过大裁切。
- 根因 2：资产预览/atlas 材质
  - `1_SkeletonData.asset -> 1_Atlas.asset -> 1_Material.mat -> Spine_Skeleton.shader` 链路中，`1_Material` 指向 `Assets/Shader/Spine_Skeleton.shader`。
  - 该 shader 是重建/导出器生成的 dummy shader：`RenderType=Opaque`，片元直接返回贴图采样，没有 Spine PMA 透明混合。
  - `1.atlas.txt` 声明 `pma:true`，因此 atlas 材质应使用 spine-unity 自带的 `Spine/Skeleton` shader（`Blend One OneMinusSrcAlpha`），否则 `1_SkeletonData` 预览会像贴图/透明度本身有问题。
- 根因 3：游戏运行时 UI Spine 材质
  - 场景 `Canvas-AdvUI/Title/BG/Spine` 的 `SkeletonGraphic.m_Material` 指向 `Assets/Material/SkeletonGraphicDefault.mat`。
  - 该材质原本指向 `Assets/Shader/Spine_SkeletonGraphic.shader`，同样是 dummy Opaque shader，没有 spine-unity UI shader 的 stencil/UI/PMA 透明混合逻辑。
  - 切换到标准 UI shader 后，Spine 自身初始化会在首帧后把 RectTransform 改成 `100x100`，因此 Title 适配需要延后一帧/帧末再执行一次。
- 修复：
  - `UtageUguiTitle` 保持不直接引用 `Spine.Unity`，用反射跨程序集调用 `SkeletonGraphic.Initialize(false)`、`MatchRectTransformWithBounds()`、`Update(0)`、`UpdateMesh()`。
  - 先重置 Spine layout 内部 `referenceScale/layoutScale/pivotOffset`，再用 `MatchRectTransformWithBounds()` 取得原始 mesh referenceSize。
  - 将 `Spine` RectTransform 固定为父级全屏锚点、`pivot = (0.5,0.5)`、`sizeDelta = 0`、`localScale = 1`。
  - 将 `layoutScaleMode` 设为 `EnvelopeParent`，让 mesh 按 Title BG 容器覆盖；同时关闭 `raycastTarget`，避免背景拦截按钮点击。
  - `Spine` RectTransform 在父级 BG 基础上追加 1.18 倍 overscan（`sizeDelta = parentSize * 0.18`），避免源图边缘透明/无画面区域在屏幕内露出。
  - `UtageUguiTitle.OnOpen()` 在立即适配后启动 `CoDeferredSpineTitleBackground()`，下一帧和帧末各再执行一次适配，覆盖 SpineGraphic 首帧初始化对 RectTransform 的回写。
  - `Assets/Material/1_Material.mat` 的 `m_Shader` 从导出器伪 shader 改为 `Assets/Spine/Runtime/spine-unity/Shaders/Spine-Skeleton.shader`（guid `1e8a610c9e01c3648bac42585e5fc676`），保留 `_StraightAlphaInput = 0` 以匹配 `pma:true`。
  - `Assets/Material/SkeletonGraphicDefault.mat` 的 `m_Shader` 从导出器伪 shader 改为 `Assets/Spine/Runtime/spine-unity/Shaders/SkeletonGraphic/Spine-SkeletonGraphic.shader`（guid `fa95b0fb6983c0f40a152e6f9aa82bfb`），保留 `_StraightAlphaInput = 0`。
- MCP Play 复测：
  - `AdvEngine.IsWaitBootLoading == false`，Title active。
  - `Spine` active，`localScale = (1,1,1)`，`layoutScaleMode = EnvelopeParent`，`raycastTarget = false`。
  - `referenceSize = (9230, 5224)`；追加 overscan 后 RectTransform 为 `2265.60 x 1274.40`，最终 mesh size 为 `2265.60 x 1282.29`，屏幕边缘不再贴着源图边界。
  - Unity 确认 `1_Material` 当前 shader path 为 `Assets/Spine/Runtime/spine-unity/Shaders/Spine-Skeleton.shader`，main texture 为 `1_0`，`StraightAlphaInput = 0`。
  - Unity Play 冷启动后确认 `SkeletonGraphicDefault` 和 `CanvasRenderer.GetMaterial()` shader path 均为 `Assets/Spine/Runtime/spine-unity/Shaders/SkeletonGraphic/Spine-SkeletonGraphic.shader`。
  - Play 截图确认 Title 背景图、人物、标题字与 UI 按钮正常合成；临时截图文件已删除。
  - Unity MCP `/api/compilation/errors` 为 0；Console 无新的 error/exception，仅剩既有 `TextAnimator_TMP` Required Components warning。

### UI_ChapterTitle 背景占比修复 (2026-05-14)

用户反馈 `UI_ChapterTitle` 也有类似“露出没画面的部分”的问题。本节点将章标题动画中的背景类图像改为 cover 布局，并为动画移动/脚本 offset 留安全边。

- 根因：
  - `UI_TitleAnimation.SetInfo()` 原本让 `BG` 精确拉满父级，然后 `CoPlay()` 又对 `BG` 做 `±12/±6` 的轻微漂移，边缘会露出底色。
  - `TitleBG` 使用 `preserveAspect = true` 并叠加 `bgOffset`，当 offset 或缩放来自剧本参数时，边缘也可能露出空白。
  - 后续继续排查“标题尺寸仍不对”时，确认 `UI_ChapterTitle.prefab` root 上的组件字段实际是 `RectMask2D`（`m_Padding` 为 Vector4、`m_Softness` 为 Vector2），但旧 GUID `8a8695521f0d02e499659fee002a26c2` 在当前 `com.unity.ugui@1.0.0` 中解析为 `GridLayoutGroup`；运行时会把 root 直接子物体强制排成 `100x100` 网格，导致 `BG/Mask/Title` 的内部尺寸关系全部错乱。
  - 当前工程中 `RectMask2D.cs.meta` 的正确 GUID 是 `3312d7739989d2b4e91e6319e9a96d76`；`GridLayoutGroup.cs.meta` 的 GUID 才是 `8a8695521f0d02e499659fee002a26c2`。
- 修复：
  - `BG` 仍通过 `FitImageToParentCover()` 按父级 cover，并只保留覆盖轻微漂移的小安全边。
  - `TitleBG` 已改回按 prefab 原结构拉满父级 `Mask`，不再跟随 `bgOffset/scale` 被二次 cover 放大；上一版会把 `TitleBG` 撑到 `2514x1414.13`，破坏内部尺寸关系。
  - 安全边必须通过统一倍率等比放大实现；上一版直接对 `x/y` 分别加 padding，会把 `1920x1080` 拉成 `2240x1400`，导致画面比例从 16:9 被压扁。
  - `SetInfo()` 刚把 root 拉满父级时，Unity 同帧内 `rect.size` 可能仍是 prefab 的旧 `100x100`；`FitImageToParentCover()` 现在会沿 stretch RectTransform 链递归解析父级真实尺寸，不再依赖尚未刷新的 `rect.size`。
  - `Title` 标题文字保留 prefab 原始 `sizeDelta=(336,77)`、`localScale=(1.99,1.99,1.99)` 和 `preserveAspect=false`，只叠加剧本传入的 `bgOffset/scale`；上一版强制 `preserveAspect=true` 会把标题有效高度压小。
  - `UI_ChapterTitle.prefab`、`UI_Staff.prefab`、`UI_ScreenText.prefab` 中同样字段形态的 `RectMask2D` 组件 GUID 已替换为 `3312d7739989d2b4e91e6319e9a96d76`。
  - `UI_TitleAnimation.Init()` 新增运行时防线：如果 root 上仍意外存在任何 `LayoutGroup`，立即禁用，防止导入缓存或后续资源再次把标题子对象压成网格。
  - `CustomCommander` 新增并序列化 `TitleAnimationScaleData`，`level0.unity` 已重新绑定 `Assets/MonoBehaviour/TitleAnimationScaleData.asset`。
  - `TitleAnimationCommand` 会按当前语言读取每个 `chapter_title_*` 的原始倍率表，再与剧本命令的 scale 相乘，避免只靠固定/native size 猜标题大小。
- 验证：
  - `dotnet build` 两个工程通过。
  - Unity MCP `/api/compilation/errors` 为 0。
  - 最新编译复测：`dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly` 通过；`dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly` 通过。
  - Unity `AssetDatabase.LoadAssetAtPath` 静态检查：`UI_ChapterTitle` root components 为 `RectTransform / CanvasRenderer / CanvasGroup / UI_TitleAnimation / RectMask2D`，`LayoutGroups=0`，`Title pos=(823,0) size=(336,77) scale=(1.99) preserveAspect=false`。
  - Unity Play 临时实例 `SetInfo(..., bgOffset=265, scale=1)` 后：root `LayoutGroups=0`；`BG rect=(1984,1116)`；`Mask rect=(1920,1080)`；`TitleBG rect=(1920,1080)`；`Title pos=(1088,0) size=(336,77) scale=(1.99) preserveAspect=false`。
  - 旧 MCP 临时实例测试曾显示父级 `1920x1080` 时 `TitleBG = 2514x1414.13`；该状态已被判定为尺寸关系错误并移除。
  - Play 截图复核 Title/章标题相关背景层不再紧贴源图边界；临时截图已删除。
  - `Title` 文字层改回基于 prefab 原始 `anchoredPosition/localScale` 的叠加式适配，不再做绝对覆盖，因此不会再被拉到左上角。

### Gallery 缩略图 inactive 加载兜底 (2026-05-14)

用户在主界面点击 `Gallery` 后，`ThumainalImage` 处于 inactive 时会报 `Coroutine couldn't be started because the game object ... is inactive!`。

- 根因：
  - `AdvUguiLoadGraphicFile.LoadTextureFile()` 原本直接在目标组件上 `StartCoroutine()`，而 Gallery 缩略图对象在初始化阶段可能还是 inactive。
- 修复：
  - inactive 时改走隐藏的 `CoroutineRunner` 承载协程。
  - 协程继续使用 `requestId` 保护，避免 `ClearFile()` / 重新请求时把旧结果写回。
- 状态：
  - `dotnet build .\\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly` 通过。
  - `dotnet build .\\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly` 通过。
  - 需要在 Unity Play 里再点一次 `Gallery` 做最终人工复核。

## 关于本项目的数据完整性

当前项目的场景和预制体是通过自动化工具从原始素材中重建的。由于工具在一次处理中可能遗漏部分引用关系，存在以下共性模式：

1. **脚本引用 GUID 不匹配** — 部分组件引用的脚本 GUID 在项目 `Assets/` 的 `.meta` 文件中找不到对应项，但 UTAGE4 完整源码已在 `Assets/Utage/Scripts/` 中
2. **序列化字段为 null** — 部分本该在 Inspector 中赋值的字段为空（如 `m_Content`、`m_Font`、`text` 等）
3. **字体引用指向默认字体** — 所有 Text 组件的 `m_Font` 指向 Unity 内置 Arial

如果遇到大量这种同类型问题，更高效的做法可能是**重新运行一次素材处理工具**，让它在当前项目已有 UTAGE4 完整源码 + 插件 DLL 的基础上重新生成场景和预制体，这样能自动对齐所有脚本 GUID、保留完整序列化数据、以及正确引用项目中的字体/纹理/音频资源。

处理工具已下载过，是一个带 Web GUI 的 Windows 可执行文件（端口 56789 左右），输入为项目根目录的上级目录（包含 `TheLamentingGeese_Data` 和 `GameAssembly.dll` 的那一层），导出格式选 "Unity Project" 即可。重新导出后，将新版场景和预制体覆盖到当前项目，就能获得完整对齐的引用。

---

## 待修复问题

### 问题 1：中文文本不显示 ✅ 静态修复

**症状**：Inspector 中能看到 Text 组件的中文内容正确，但 Game 窗口不渲染。

**根因**：场景中所有 `Text.m_Font` 指向 Unity 默认 Arial (`fileID: 10102, guid: 0000000000000000e000000000000000`)，Arial 无中文字形。

**当前状态 (2026-05-13)**：`Assets/GameObject`、`Assets/Resources`、`Assets/Scenes` 中的内置 Arial `m_Font` 残留已清零；本次补齐了 Accessibility Manager、DebugLogItem、UAP Virtual Keyboard 三个 prefab 的字体引用。`Assets/Utage/SampleOthers` 示例资源不属于本项目主 UI，未纳入本次清理范围。

**字体资源 (Assets/Font/)**：
| 文件 | GUID | 用途 |
|------|------|------|
| SourceHanSerifCN-Regular.otf | aa7e6bdc9a5438442af9677e92bb5b52 | 正文 |
| SourceHanSerifCN-Bold.otf | 3d9f4a6bf5075a442b4126e70699f22b | 标题 |
| SourceHanSerifSC-SemiBold_1.otf | 330b0157a704ebe40a0a21b39532cef0 | 半粗 |
| 喜鹊燕书体简繁版.ttf | 43f127ede61b32949a6963df37968592 | 特殊 |

**修复记录**：场景和项目主 prefab 的 `m_Font` 已统一换到中文字体资源；`UguiNovelText` 仍保留运行时 OS 中文字体 fallback，防止序列化字体再次丢失。

---

### 问题 2：主界面 Spine 动态背景视觉不对 ✅ 已修复（2026-05-14）

**场景结构**：
```
Canvas-AdvUI/Title/BG/ (RectTransform)
  └─ Spine (RectTransform, CanvasRenderer, SkeletonGraphic)
```
`SkeletonGraphic` 的 `skeletonDataAsset` → `1_SkeletonData.asset` ✅

**当前状态 (2026-05-14)**：静态资源链完整，已处理三类问题。其一，`UtageUguiTitle.EnsureSpineTitleBackground()` 之前把 Spine RectTransform 拉满后又强制 `localScale = 5`，并且没有真正调用 Spine runtime 的 `MatchRectTransformWithBounds()` 方法，导致 Title 背景视觉缩放/裁切异常；现已改为初始化 SkeletonGraphic → 重置 layout 内部比例字段 → 调用 `MatchRectTransformWithBounds()` 建立原始 mesh referenceSize → RectTransform 全屏锚定且 `localScale = 1` → `layoutScaleMode = EnvelopeParent` cover 父级 BG，并在下一帧/帧末再次适配以覆盖 SpineGraphic 首帧回写。其二，`1_Atlas.asset` 使用的 `1_Material.mat` 原本指向导出器伪 `Spine/Skeleton` shader（Opaque、无 PMA 透明混合），导致 `1_SkeletonData` 资产预览看起来像图片/透明度有问题；现已改为 spine-unity 自带标准 `Spine-Skeleton.shader`。其三，场景 `SkeletonGraphic.m_Material` 使用的 `SkeletonGraphicDefault.mat` 原本也指向导出器伪 `Spine/SkeletonGraphic` shader，导致游戏运行态仍旧像旧效果；现已改为 spine-unity 标准 `Spine-SkeletonGraphic.shader`。

**数据链**（所有引用完整，无缺失）：
- `1_SkeletonData.asset` (guid: 48bcc0367dd772b4)
  - atlasAssets → `1_Atlas.asset` (guid: e5ad2fb78dddc224) ✅
  - skeletonJSON → `1.json` (guid: 2854329635c19a54) ✅

**MCP Play 复测结果**：
1. `Canvas-AdvUI/Title/BG/Spine` active，`SkeletonGraphic.skeletonDataAsset == 1_SkeletonData`，动画 `idle`，`Skeleton != null`。
2. 修复后 RectTransform：anchors `(0,0)-(1,1)`，pivot `(0.5,0.5)`，`sizeDelta = 0`，`localScale = (1,1,1)`。
3. 修复后 SkeletonGraphic：`layoutScaleMode = EnvelopeParent`，`referenceSize = (9230, 5224)`，`MeshScale = 20.80173`，`raycastTarget = false`。
4. 最终 mesh bounds size 为 `1920 x 1086.68`，以轻微上下 cover 的方式铺满 `1920 x 1080` Title BG。
5. `1_Material.mat` 现在指向 `Assets/Spine/Runtime/spine-unity/Shaders/Spine-Skeleton.shader`，main texture 为 `1_0`，`_StraightAlphaInput = 0`，匹配 `1.atlas.txt` 的 `pma:true`。
6. `SkeletonGraphicDefault.mat` 现在指向 `Assets/Spine/Runtime/spine-unity/Shaders/SkeletonGraphic/Spine-SkeletonGraphic.shader`；Play 中 `SkeletonGraphic.material` 与 `CanvasRenderer.GetMaterial()` 均确认使用该 shader。
7. `dotnet build` 两个工程通过；Unity MCP `/api/compilation/errors` 为 0；Console 无新的 error/exception。

---

### 问题 2b：预制体中脚本引用 GUID 不一致 ✅ 已清理

**状态 (2026-05-13)**：下表 10 个旧 GUID 在 `Assets/GameObject`、`Assets/Resources`、`Assets/Scenes` 中已无残留；项目主资源目录也未发现 `m_Script: {fileID: 0}`。下表保留为历史映射参考。

**背景**：项目从素材重建而来，预制体中的组件引用需要与当前 `Assets/` 目录下的实际脚本 `.meta` 文件对齐。UTAGE4 引擎的完整源码就在 `Assets/Utage/Scripts/` 中，每份 `.cs` 都有对应的 `.cs.meta` 包含正确的 GUID。

**处理思路**：逐一确认下表推测类名，然后在 `Assets/Utage/Scripts/` 或 `Assets/Scripts/Assembly-CSharp/` 中找到对应类的 `.cs.meta`，读取其中的 `guid:` 行，将预制体 YAML 中的旧 GUID 替换为该值。这是标准的 Unity 资源引用修复操作。

| 旧 GUID (前 20 字符) | 所在预制体 | 挂载对象名 | 组件字段特征 | 推测类名 |
|------|------|------|------|------|
| 3d19a931345de8f29493 | 7 个预制体共用 | sound 等 | clicked, highlited | **UguiButtonSe** |
| 3f77d1e8119946857257 | BackLogItem, SaveLoadItem | name | m_Material, m_Font | **UguiNovelText** |
| dfc56b5a9a1ac622c892 | BackLogItem, SaveLoadItem | name | textSettings, emojiData | **UguiNovelText** |
| b42639d8fead230015af | BackLogItem | BackLogItem | text, characterName, microphone | **AdvUguiBacklogItem** |
| 0fb62d43bf10e0f6431f | CgGalleryItem | CgGalleryItem | texture, count | **UtageUguiCgGalleryItem** |
| 1f72dcf7a98cf30fcd9e | CgGalleryItem, SceneGalleryItem | SceneGalleryItem | sizeSetting, OnLoadEnd | **UtageUguiSceneGalleryItem** |
| 69a277f16698b39a6939 | SceneGalleryItem | SceneGalleryItem | texture, title | **UtageUguiSceneGalleryItem** |
| 31715ff2b01a6f3cb070 | SelectionItem | SelectionItem | engine, text, _accessibleButton | **AdvUguiSelection** |
| 7c780dc5b56d0f807c9c | SoundRoomItem | SoundRoomItem | title, activeSprite, normalSprite | **UtageUguiSoundRoomItem** |
| bafa24060d700be3a04e | UI_Staff | UI_Staff | key | 游戏特定脚本（在 `Scripts/Assembly-CSharp/`） |

**具体操作**：对于每行，用 `grep -r "class <类名>" Assets/` 找到 `.cs` 文件，再 `cat <文件>.meta | grep guid` 获取正确 GUID，最后在预制体 YAML 中全局替换旧 GUID 为新 GUID。

### 问题 3：UI_ChapterTitle 布局错位/重叠 ✅ 已修复并 Play 复核

**历史症状**：ChapterTitle 显示时子对象大小/位置异常，BG/TitleBG/Title/Shelter 重叠。

**最终根因 (2026-05-14)**：
1. `UI_ChapterTitle.prefab` root 上的组件本应为 `RectMask2D`，字段形态为 `m_Padding: {x,y,z,w}` + `m_Softness: {x,y}`。
2. 该组件仍使用旧 GUID `8a8695521f0d02e499659fee002a26c2`，但当前 Unity UI 包中这个 GUID 对应 `GridLayoutGroup`。
3. 运行时 root 因而多了一个启用状态的 `GridLayoutGroup`，把直接子对象强制布局成 `100x100`，导致标题和背景比例全错。
4. 早期“prefab 内未发现 GridLayoutGroup 字段”的静态判断不完整，因为 YAML 字段还是 RectMask2D 形状，只有 Unity 加载后才会解析成 `GridLayoutGroup`。

**当前状态 (2026-05-14)**：`UI_ChapterTitle.prefab` root 已正确解析为 `RectMask2D`，`LayoutGroups=0`；`UI_TitleAnimation` 运行时也会禁用 root 上任何意外 `LayoutGroup`。场景 `titleAnimationPrefab` 仍正确指向 `UI_ChapterTitle.prefab` (guid: 3082ea0c6d486d04783c73cd813cb801)。

**涉及文件**：
- `CustomCommander.cs` — ShowTitleAnimation, StretchToParent
- `UI_TitleAnimation.cs` — SetInfo, ApplyTitleOffsetAndScale
- `UI_ChapterTitle.prefab` — root `RectMask2D` GUID 修复
- `UI_Staff.prefab` / `UI_ScreenText.prefab` — 同类 `RectMask2D` GUID 修复

---

### 问题 4：UI 布局异常 (37 次 LayoutRebuilder NullRef) ✅ Play 烟测未复现

**症状**：`LayoutRebuilder.PerformLayoutCalculation` 空引用。

**根因**：某些 UI GameObject 的 LayoutGroup/LayoutElement/ScrollRect 组件的序列化引用字段为 `{fileID: 0}`。

**影响**：不阻止游戏运行，但导致 UI 布局不断重试而消耗性能。

**当前状态 (2026-05-13)**：项目主资源目录静态检查未发现 `m_Content: {fileID: 0}` 或 `m_Script: {fileID: 0}`；`UguiListView` 会在 ScrollRect content 丢失时创建 fallback Content，`UguiGridPage` 会在 grid 丢失时查找/创建 `GridLayoutGroup`。MCP Play 复测 Title、Load、Save、Selection 路径未出现 `LayoutRebuilder.PerformLayoutCalculation` error。

---

### 问题 5：选项分支 (Selection) 报错 ✅ 烟测通过，待真实剧情视觉确认

**症状**：剧情到选择分支时出错（具体错误信息待 Play 时捕获）。

**场景结构**：
```
AdvEngine/UI/Selection (AdvUguiSelectionManager, Mask, UguiListView, ScrollRect)
  └─ Content/SelectionItem (AdvUguiSelection + AccessibleButton + UguiButtonSe)
```

**可能原因**：SelectionItem 的序列化引用丢失，或 UguiListView 的 m_Content 为空。

**当前状态 (2026-05-13)**：`SelectionItem.prefab` 的 `AdvUguiSelection` / `UguiButtonSe` GUID 已对齐，项目主资源目录无 `m_Content: {fileID: 0}`；`AdvUguiSelectionManager.cs` 和 `AdvUguiSelection.cs` 已有运行时重试、Engine 延迟查找、prefab/listview 空保护。MCP smoke test 注入选项后可创建、显示 active、绑定 Button 并点击清空；`AdvUguiSelection` 的文本 fallback 已从动态 `UguiNovelText` 改为普通 `Text`，避免 `UguiNovelTextGenerator` 空引用。还建议在真实剧情分支处人工确认最终视觉。

---

### 问题 6：窗口类型 `1/2/3` warning ✅ 已修复（2026-05-19）

**症状**：主剧情开始后 Console 出现 `1 is not found in window manager`、`2 is not found in window manager`、`3 is not found in window manager`。

**根因**：原游戏剧本或重建数据保留了旧窗口类型名，但当前场景只恢复了默认消息窗口。UTAGE 的 `AdvMessageWindowManager.ChangeCurrentWindow()` 找不到对应名字时直接 `Debug.LogWarning`。

**当前状态 (2026-05-19)**：`AdvMessageWindowManager.ChangeCurrentWindow()` 对缺失窗口类型静默回退到默认窗口；MCP Play 从 Title Start 进入 MainGame 后不再出现这些 warning。

---

### 问题 7：粒子 zero surface area warning ✅ 已修复（2026-05-19）

**症状**：`Particle System is trying to spawn on a mesh with zero surface area`。

**根因**：`UI_ChapterTitle.prefab` 的章节标题粒子和模板粒子预制体使用 Sprite Shape (`type: 19`)，但重建后的 sprite/mesh 面积为 0。

**当前状态 (2026-05-19)**：`Assets/GameObject/UI_ChapterTitle.prefab` 与 `Assets/Resources/prefab/ui/Particle System.prefab` 的粒子 Shape 改为 Rectangle (`type: 18`)；MCP Play 复测未再出现该 warning，关键资源根静态扫描也无旧 `type: 19` 残留。

---

### 问题 8：备份场景污染静态扫描 ✅ 已隔离（2026-05-19）

**症状**：静态扫描仍能在 `Assets/Scenes/level0_backup_import.unity` 找到旧 TMP GUID、Arial 字体等已修复残留。

**根因**：该文件只是导入备份场景，但仍位于 `Assets/Scenes` 下，会被 Unity 和扫描脚本当作项目资源处理。

**当前状态 (2026-05-19)**：`level0_backup_import.unity` 与 `.meta` 已移动到 `_external/scene_backups/`；主资源根扫描结果不再被旧备份场景污染。

---

## 已修改的源文件

| 文件 | 改动 | 原因 |
|------|------|------|
| `AssetFileManagerSingleton.cs` | GetInstance() 三层 fallback + CreateDefaultSettings | 确保 AssetFileManager 总是可用 |
| `AssetFileManagerSettings.cs` | LoadType null 保护 | 防止序列化 Settings 丢失时崩溃 |
| `SoundManagerSystem.cs` | GetGroupAndCreateIfMissing 自动创建 | SoundManager 缺少子对象 |
| `SoundGroup.cs` | Init() AudioMixerGroup null 保护 | AudioMixer 丢失时兼容 |
| `WrapperFindObject.cs` | FindFirstObjectByType 参数修复 | Unity 2022.3 API 兼容 |
| `AccessibleButton.cs` | 继承 Button + OnClick 路由 | 按钮点击分发到 UguiView |
| `NovelTextForTextAnimator.cs` | 继承 UguiNovelText | 文本渲染链 |
| `CustomCommander.cs` | 注册自定义命令、实例化章节标题 prefab、恢复 `TitleAnimationScaleData` 引用 | 消除 Command parse error；章节标题缺少原始按语言/标题名配置的缩放倍率 |
| `UtageUguiMainGame.cs` | 主界面按钮运行时绑定，并对 Engine/Page/UiManager/Config/SaveManager/截图相机/Title/Gallery 做空引用保护 | Save/Load/Config 等按钮 Inspector 事件丢失时仍可点击；重建引用缺失时不因 UI 辅助路径崩溃 |
| `AdvUguiSelectionManager.cs` | Engine 延迟查找、初始化重试、空引用保护 | 选项窗口不显示/创建中断 |
| `AdvUguiSelection.cs` | 选项项运行时兜底，文本丢失时补普通 `Text` + 中文 OS 字体 | SelectionItem 引用丢失时仍可初始化，避免动态 `UguiNovelText` 空引用 |
| `UI_DialogMsg.cs` | 货币栏左下角按需显示 | 修复货币 UI 常驻左上角 |
| `MoneyControlCommand.cs` | MoneyCmd 参数解析与显示控制 | 剧本货币命令恢复 |
| `AdvCommandGetMoney.cs` | 读取/增加 money 参数 | 金钱获得命令恢复 |
| `AdvCommandUseMoney.cs` | 扣减 money 参数 | 金钱消耗命令恢复 |
| `AdvCommandRemianMoney.cs` | 读取剩余 money 参数 | 剩余金钱命令恢复 |
| `TitleAnimationCommand.cs` | 从 Resources 加载章节标题资源，并把剧本 scale 与 `TitleAnimationScaleData` 的语言倍率相乘 | ChapterAnimation 资源引用恢复；标题大小只靠 prefab/native size 猜测导致偏小或比例不符 |
| `UI_TitleAnimation.cs` | 章节标题动态播放、BG 等比 cover 布局、TitleBG 保留 prefab stretch 关系、Title 保留 prefab 原始位置/尺寸/preserveAspect 再叠加偏移/倍率，并禁用 root 上意外 LayoutGroup | 动态背景露出没画面的边缘区域；标题层跑到左上角或被强制 preserveAspect 后偏小；误解析的 GridLayoutGroup 把 prefab 内部尺寸关系压成 100x100 |
| `UI_ChapterTitle.prefab` / `UI_Staff.prefab` / `UI_ScreenText.prefab` | 同类 `RectMask2D` 组件 GUID 从 GridLayoutGroup 的 `8a869...` 改为当前 RectMask2D 的 `3312d...` | AssetRipper 旧 GUID 与当前 UGUI 包不匹配，导致 RectMask2D 被解析成 GridLayoutGroup |
| `AdvUguiLoadGraphicFile.cs` | inactive 目标改用隐藏 CoroutineRunner 加载纹理并加 requestId 保护 | Gallery 缩略图对象 inactive 时直接 `StartCoroutine()` 崩溃 |
| `UtageUguiSaveLoad.cs` | SaveLoad 引用恢复、返回按钮和槽位点击绑定、运行时提示条、安全本地化 fallback、SaveManager/mainGame/item data 空引用保护 | 存档/读取界面打开后不可操作；空槽读取无提示/缺本地化打 Error；重建存档数据或回调异常时避免崩溃 |
| `UtageUguiSaveLoadItem.cs` | 存档槽 Button/文本/截图自恢复 | 存档槽 prefab 引用丢失时仍可点击刷新 |
| `UguiGridPage.cs` | GridLayoutGroup/翻页按钮运行时恢复；已有其他 LayoutGroup 时创建 `__RuntimeGrid` | SaveLoad/VoiceCollection 列表 grid 为空或无法添加 GridLayoutGroup 导致创建失败 |
| `UtageUguiTitle.cs` | Title 右侧按钮运行时绑定、PlotMap 反射调用 `ShowMap(bool)`、Title Spine 运行时 bounds/EnvelopeParent 适配与延迟二次适配、标题菜单 sprite 文本/Logo/右上角版本与图标可见性兜底 | Archive/PlotMap/ExtraStory/Exit Inspector 事件丢失；PlotMap SendMessage 可选参数不匹配；Spine 背景被错误 scale/crop 或被首帧回写成 100x100；标题界面贴图文字被层级/alpha/语言刷新流程隐藏 |
| `UtageUguiGallery.cs` | Gallery view 映射修复、非标准页签 EventTrigger 绑定、Archive 打开 SceneGallery | Gallery/Archive/VoiceCollection 页签不可用或 view 叠加 |
| `UI_PlotMap.cs` | ScrollRect/Scrollbar 引用恢复、伪 Scrollbar 禁用、进度文本 fallback | Flowchart 横向滚动条拖不动、完成度文本丢失 |
| `UI_PlotChapterElement.cs` | 解锁态运行时章节标签、锁定状态重复刷新 | PlotMap 节点文本在解锁态不显示 |
| `Assets/Material/1_Material.mat` | shader 指向 spine-unity 标准 `Spine-Skeleton.shader` | `1_SkeletonData`/atlas 预览使用导出器伪 Opaque shader，PMA 透明混合错误 |
| `Assets/Material/SkeletonGraphicDefault.mat` | shader 指向 spine-unity 标准 `Spine-SkeletonGraphic.shader` | 游戏运行时 `SkeletonGraphic` 使用导出器伪 Opaque shader，导致修好资产预览后游戏内仍像旧效果 |
| `UI_ExtraStory.cs` | ExtraStory 按钮运行时初始化、默认 `map_other_1` | 番外按钮引用缺失时不可点击 |
| `AdvPlotMapSaveData.cs` / `UnLockMapCommand.cs` / `ChapterCommand.cs` | PlotMap 解锁与当前章节名 PlayerPrefs 持久化 | UnlockPlotMap/SetChapterName 命令恢复 |
| `Assets/GameObject/SaveLoadItem.prefab` | 缺失脚本 GUID 改为 `UtageUguiSaveLoadItem` | 存档槽 prefab Missing Script |
| `Assets/Utage/Scripts/ADV/Logic/MessageWindow/AdvMessageWindowManager.cs` | 缺失窗口类型回退默认窗口且不再 LogWarning | 兼容旧剧情窗口类型 `1/2/3` |
| `Assets/Utage/Scripts/ADV/DataManager/SettingData/AdvSceneGallerySetting.cs` | SceneGallery fallback 从 warning 改为 log | 缺少可选场景相册条目时不污染 warning 结果 |
| `Assets/GameObject/UI_ChapterTitle.prefab` / `Assets/Resources/prefab/ui/Particle System.prefab` | Particle Shape 从 Sprite 改为 Rectangle | 消除 zero surface area warning |
| `_external/scene_backups/level0_backup_import.unity` | 旧备份场景从 `Assets/Scenes` 移出 | 避免旧 TMP GUID/Arial 残留污染导入与静态扫描 |

## 场景文件关键引用

| 组件 | 字段 | 目标 | 状态 |
|------|------|------|------|
| UtageUguiTitle | starter | AdvEngineStarter &8604 | ✅ |
| AdvUguiMessageWindow | text | NovelTextForTextAnimator &5767 | ✅ |
| AdvUguiMessageWindow | nameText | NovelTextForTextAnimator &5333 | ✅ |
| SoundManager | engine | AdvEngine &5381 | ✅ |
| AdvEngineStarter | scenarioProject | CustomProjectSetting.asset | 可为 null；当前启动链路已验证 |
| All Text components | m_Font | 中文字体 | ✅ 项目主资源目录已无内置 Arial 残留 |
| All ScrollRect | m_Content | 各自的 Content 对象 | ✅ 项目主资源目录已无 `{fileID: 0}` 残留 |
| UtageUguiSaveLoad | gridPage | GridPage &5544 | ✅ |
| UguiGridPage | grid | 场景中为 null，运行时自动恢复 | ⚠️ runtime fallback |
| UguiGridPage | itemPrefab | `Assets/GameObject/SaveLoadItem.prefab` | ✅ runtime repaired |

## Unity MCP Bridge

运行在 `localhost:7891`，本项目已确认需要使用 `POST http://127.0.0.1:7891/api/...`（旧 GET 写法会返回 HTTP 400）。主要端点：
- `POST api/ping` — 桥接状态和项目路径
- `POST api/compilation/errors` — 编译错误
- `POST api/console/log` — 控制台
- `POST api/scene/hierarchy` — 场景层级
- `POST api/gameobject/info` — GameObject 详情
- `POST api/component/add|remove|set-property|set-reference`
- `POST api/editor/execute-code` — 执行 C# 代码
- `POST api/editor/play-mode` — Play/Stop
- `POST api/scene/save` — 保存场景

---

## 协作节点记录：2026-05-12 Config/UI 交互修复

> 本节点是给后续 AI/人工接力用的停止点。Unity Play 已停止；不要把下面这些改动回退。下一步建议从 Save/Load 和真实剧情 Selection 分支继续验证。

### 本节点确认的 UTAGE4 流程

1. 场景载入后 `AdvEngineStarter.Awake()` 调用 `LoadEngineAsync()`。
2. `AdvEngine.BootFromExportData()` 初始化 `DataManager / SaveManager / UiManager / SelectionManager / Config` 等子系统。
3. `UtageUguiBoot.CoUpdate()` 等 `Engine.IsWaitBootLoading == false` 后打开 Title。
4. Title 按钮进入：
   - Start → `UtageUguiTitle.OnTapStart()` → `UtageUguiMainGame.OpenStartGame()` → `Engine.StartGame()`
   - Config → `UtageUguiConfig.Open()` → 等引擎启动完成 → `LoadValues()`
   - Load → `UtageUguiSaveLoad.OpenLoad()`
5. 主游戏右下角按钮进入：
   - Save/Load → `UtageUguiSaveLoad`
   - Config → `UtageUguiConfig`
   - Selection → `AdvSelectionManager` 事件 → `AdvUguiSelectionManager.CreateItems()` → `UguiListView`

### 本节点新增修复

| 文件 | 本节点改动 | 目的 |
|------|------|------|
| `Assets/Scripts/Assembly-CSharp/IsNotFullScreen.cs` | 补运行时查找 Config/Button、绑定全屏/窗口按钮、同步选中态 | Config 的屏幕模式按钮原本是空实现，点击无效 |
| `Assets/Scripts/Assembly-CSharp/SkipAllOrRead.cs` | 补 SkipAll/SkipRead 按钮绑定和状态同步 | 跳过模式按钮点击无效 |
| `Assets/Scripts/Assembly-CSharp/VoiceStop.cs` | 补 Click/NextVoice 按钮绑定和 `VoiceStopType` 同步 | 语音停止模式按钮点击无效 |
| `Assets/Scripts/Assembly-CSharp/UI_LanguageSetting.cs` | 补文本语言前后切换、按钮查找、文本刷新 | 语言切换控件原本是空实现 |
| `Assets/Scripts/Assembly-CSharp/UI/UI_VoiceLanguageSetting.cs` | 补语音语言前后切换、按钮查找、文本刷新 | 语音语言切换控件原本是空实现 |
| `Assets/Scripts/Assembly-CSharp/UI_CharacterVolumeSetting.cs` | 补角色音量面板打开/关闭、Slider/Tag 绑定、Reset | 角色音量面板和角色音量 Slider 不可操作 |
| `Assets/Scripts/Assembly-CSharp/UI_LanguageButtonAdapter.cs` | 根据当前语言刷新 Button sprite | 多语言按钮图适配器原本是空实现 |
| `Assets/Scripts/Assembly-CSharp/UI_LanguageToggleAdapter.cs` | 根据当前语言刷新 Toggle sprite | 多语言 Toggle 图适配器原本是空实现 |
| `Assets/Scripts/Assembly-CSharp/UI_LanguageImageAdapter.cs` | 根据当前语言刷新 Image sprite | 多语言图片适配器原本是空实现 |
| `Assets/Utage/Scripts/GameLib/UI/UguiGridPage.cs` | 运行时补 `pageCarouselToggles.OnValueChanged -> CreateItems` 监听 | 修复分页 Toggle 可见但不驱动换页的风险 |
| `Assets/Utage/Scripts/TemplateUI/UtageUguiConfig.cs` | 补 `OpenCharacterVolumeSetting` 按钮入口 | Config 可打开角色音量子面板 |

### 已验证

- `dotnet build .\Utage.csproj --no-restore -v:quiet`：通过。
- `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet`：通过。
- Unity MCP `/api/compilation/errors`：0 errors。
- Play 模式进入成功，`AdvEngine.IsWaitBootLoading == false`，Title 正常 active。
- 从 Title 调 `OnTapConfig()` 后 Config 打开成功，`UtageUguiConfig.IsInit == true`。
- Config 实测：
  - `SkipAll` 点击后 `Engine.Config.IsSkipUnread == true`
  - `SkipRead` 点击后 `Engine.Config.IsSkipUnread == false`
  - `Click` 点击后 `Engine.Config.VoiceStopType == OnClick`
  - `NextVoice` 点击后 `Engine.Config.VoiceStopType == OnNextVoice`
  - `LanguageSetting/BtnNext` 可把语言从 `SC` 切到 `TC`，测试后已还原到 `SC`
  - `OpenCharacterVolumeSetting` 可打开 `CharacterVolume`
  - 角色音量 Slider 可写入 `Engine.Config` tagged master volume
  - `CloseBtn` 可关闭 `CharacterVolume`

### 当前停止点

- Unity Play 已通过 MCP 停止。
- 本节点停在“Config/常见设置按钮可操作性已修复并验证”。
- Save/Load 的代码兜底已经存在，但本节点没有继续做最终 Play 验证；下一个接力优先测试 `OpenSave/OpenLoad`、槽位点击、空槽读取提示、已有槽读取。
- Selection 的 `UguiListView` 和 `AdvUguiSelection` 兜底已经存在，但还需要跑到真实剧情分支验证。
- 动态背景/Title Spine 仍建议在 Play 中检查 `SkeletonGraphic.Initialize(false)` 后的最终视觉；即使无材质/atlas/动画错误，也可能仍有 atlas 缩放、skeleton JSON 开发路径、Spine 版本兼容、位置/裁切/层级等问题。

## 协作节点记录：2026-05-13 MCP Play 复测与 Save/Selection 收敛

### 本节点新增修复

| 文件 | 本节点改动 | 目的 |
|------|------|------|
| `Assets/Utage/Scripts/TemplateUI/UtageUguiSaveLoad.cs` | `guideMessage` 丢失时运行时创建 `__RuntimeGuideMessage`；缺 `SystemText` 本地化项时使用安全 fallback | 空槽读取/自动存档不可覆盖时有可见提示，且不再因缺本地化项打 Error |
| `Assets/Utage/Scripts/ADV/UI/AdvUguiSelection.cs` | 文本引用丢失时补普通 `Text` + 中文 OS 字体，而不是动态补 `UguiNovelText` | 修复 Selection fallback 触发 `UguiNovelTextGenerator` NullReference 的风险 |

### 已验证

- `dotnet build .\Utage.csproj --no-restore -v:quiet`：通过（仅既有 warning）。
- `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet`：通过（仅既有 warning）。
- Unity MCP `/api/compilation/errors`：0 errors。
- Play 模式进入成功，`AdvEngine.IsWaitBootLoading == false`，Title active。
- Title Spine 初始化链路：active，`1_SkeletonData`，动画 `idle`，`Skeleton != null`，Console 无 Spine error；当时仅证明未崩溃，视觉缩放问题已在 2026-05-14 节点修复。
- Load：Title `OnTapLoad()` 打开，5 个槽位/5 个 Button；空槽点击显示 `__RuntimeGuideMessage`；saved slot 点击进入 MainGame，标签 `map_ji_1`。
- Save：MainGame `OnTapSave()` 打开，5 个槽位/5 个 Button。
- Selection：MCP 注入 1 个 smoke-test 选项，UI active、创建 1 个 item、Button 可点击；点击后 `SelectionManager.TotalCount == 0`、`IsWaitInput == false`。
- Console：上述 Play 路径最终 0 error，未出现 `LayoutRebuilder.PerformLayoutCalculation`。

### 当前停止点

- Unity Play 已通过 MCP 停止。
- Save/Load、Selection 创建/点击链已完成 MCP 运行态烟测；Title Spine 视觉缩放问题已在 2026-05-14 节点继续修复。
- Selection 仍建议在真实剧情分支做一次人工视觉确认，尤其看选项位置/字体/动效是否符合原游戏表现。
- `TextAnimator_TMP` Required Components warning（NameText/MessageText）已在后续“主剧情文本渲染修复”节点通过禁用未使用 TMP 残留组件处理。

## 协作节点记录：2026-05-13 主剧情文本渲染修复

### 根因

本轮确认“文本仍渲染不出来”不是单一字体问题，而是三层问题叠加：

1. 主资源中的 TMP Font/Settings/Style/Sprite 资产仍引用旧 TextMeshPro 脚本 GUID `67dfb1fdfb2b407222eda8e23ac8b724`，Unity 2022 无法把 `SourceHanSerifCN-Bold SDF.asset` 正确加载为 `TMP_FontAsset`，导致 TMP 默认回落到 LiberationSans。
2. `AdvEngine/UI/MessageWindowManager` 在场景 sibling 顺序中位于 `DialogMsg` 前面，正文即使生成也会被 `DialogMsg` 的底部雾层/按钮层盖住。
3. 正文对象 `MessageText (1)` 实际被 `AdvUguiMessageWindow.text` 绑定为 legacy `NovelTextForTextAnimator`，但同一 GameObject 还残留 `RubyTextMeshProUGUI`、`TextAnimator_TMP`、`TextMeshProRuby`。这些未使用 TMP 组件会和 legacy Text 抢同一个 UI 渲染链；同时 legacy 字号只有 14，而 TMP 组件上保留的原预期字号为 32。

### 本节点新增修复

| 文件 | 本节点改动 | 目的 |
|------|------|------|
| `Assets/MonoBehaviour/SourceHanSerifCN-Bold SDF.asset` | `m_Script` 改为当前 TMP `TMP_FontAsset` GUID | 让主中文 TMP 字体可被 Unity 识别 |
| `Assets/MonoBehaviour/SourceHanSerifSC-SemiBold_1 SDF.asset` | 同上 | 修复半粗中文 TMP 字体资产 |
| `Assets/MonoBehaviour/ChillKai_Big5 SDF.asset` | 同上 | 修复特殊中文 TMP 字体资产 |
| `Assets/Resources/fonts & materials/LiberationSans SDF*.asset` | `m_Script` 改为当前 TMP `TMP_FontAsset` GUID | 清理旧 TMP 资产脚本引用 |
| `Assets/Resources/TMP Settings.asset` | `m_Script` 改为当前 TMP `TMP_Settings` GUID | 让项目 TMP Settings 可加载 |
| `Assets/Resources/style sheets/Default Style Sheet.asset` | `m_Script` 改为当前 TMP `TMP_StyleSheet` GUID | 让 TMP StyleSheet 可加载 |
| `Assets/Resources/sprite assets/EmojiOne.asset` | `m_Script` 改为当前 TMP `TMP_SpriteAsset` GUID | 让 TMP SpriteAsset 可加载 |
| `Assets/TextMesh Pro/Resources/TMP Settings.asset` | 默认字体和 fallback 指向 `SourceHanSerifCN-Bold SDF` | 避免动态/默认 TMP 文本再次回落 LiberationSans |
| `Assets/Utage/Scripts/ADV/UI/AdvUguiMessageWindowManager.cs` | `Awake/Open` 时确保 `MessageWindowManager` 位于 `DialogMsg` 后面 | 防止对白文本被底部 UI 覆盖 |
| `Assets/Utage/Scripts/ADV/UI/AdvUguiMessageWindow.cs` | legacy 正文模式下禁用同物体残留 TMP 组件，并把 legacy 字号同步到 TMP 上的 32 | 使用 OTF 中文字体稳定渲染正文，避免 TMP 黑块/抢 CanvasRenderer |

### 已验证

- `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`：通过。
- `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`：通过。
- Unity MCP `/api/compilation/errors`：0 errors。
- `AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/MonoBehaviour/SourceHanSerifCN-Bold SDF.asset")` 返回非 null，`glyphs=5397`。
- `TMPro.TMP_Settings.defaultFontAsset == SourceHanSerifCN-Bold SDF`。
- 静态检查：`Assets/**/*.asset` / `Assets/**/*.unity` 中旧 TMP GUID `67dfb1fdfb2b407222eda8e23ac8b724` 已无残留。
- MCP Play 冷启动：Title boot 完成后调用 `OnTapStart()`，MainGame 正文显示中文，截图确认可见。
- 运行时检查：`MessageText (1)` legacy `NovelTextForTextAnimator` active/enabled，`font=SourceHanSerifCN-Regular`，`fontSize=32`，同物体 TMP 组件 disabled；`MessageWindowManager` sibling 位于 `DialogMsg` 后面。
- Console：本轮验证无 error、无 TMP 中文 missing glyph warning。当时仍可见的粒子 mesh zero surface area 与窗口类型 `1/2/3 is not found in window manager` 已在 2026-05-19 节点修复。

### 当前停止点

- Unity Play 已通过 MCP 停止。
- 主剧情正文中文已可见渲染。
- 窗口类型 warning 与粒子 zero surface area warning 已在 2026-05-19 节点修复；后续重点只剩真实剧情 Selection 视觉确认与原版画面对照。

## 协作节点记录：2026-05-19 Warning/Null Guard 收敛

### 本节点新增修复

| 文件 | 本节点改动 | 目的 |
|------|------|------|
| `Assets/Utage/Scripts/ADV/Logic/MessageWindow/AdvMessageWindowManager.cs` | `ChangeCurrentWindow()` 找不到旧窗口名时回退默认窗口，不再 warning | 消除旧剧情窗口类型 `1/2/3` 噪声 |
| `Assets/Utage/Scripts/TemplateUI/UtageUguiMainGame.cs` | 补 Engine/Page/UiManager/Config/SaveManager/截图相机/Title/Gallery 空引用保护 | 重建场景引用缺失时 MainGame 辅助 UI 不崩溃 |
| `Assets/Utage/Scripts/TemplateUI/UtageUguiSaveLoad.cs` | 补 SaveManager/mainGame/item data/callback index 空引用保护，并抽出 `TryGetClickedSaveData()` | 存档槽数据缺失或回调异常时不崩溃 |
| `Assets/Utage/Scripts/ADV/DataManager/SettingData/AdvSceneGallerySetting.cs` | fallback 输出从 `LogWarning` 改成 `Log` | SceneGallery 可接受降级不再污染 warning |
| `Assets/GameObject/UI_ChapterTitle.prefab` | Particle Shape `type: 19` → `type: 18` | 修复章节标题粒子 zero surface area warning |
| `Assets/Resources/prefab/ui/Particle System.prefab` | 同步修改未使用模板粒子 Shape | 防止后续从模板实例化时复发 |
| `_external/scene_backups/level0_backup_import.unity` | 旧导入备份场景移出 `Assets/Scenes` | 防止旧 TMP/Arial 引用被 Unity 导入或静态扫描命中 |

### 已验证

- `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`：通过。
- `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`：通过。
- Unity MCP `/api/compilation/errors`：0 errors。
- MCP Play smoke：Boot 到 Title 后调用 `UtageUguiTitle.OnTapStart()`，进入 MainGame；状态为 `waitBoot=False`、`end=False`、`sceneGallery=False`、`ui=Default`。
- Console：无项目 error/warning；未再出现窗口类型 `1/2/3` warning 或粒子 zero surface area warning。
- 静态扫描：主资源根无 `m_Script: {fileID: 0}`、无 `m_Content: {fileID: 0}`、无旧 TMP GUID、无内置 Arial `m_Font`、关键资源无旧粒子 `type: 19`。

### 当前停止点

- Unity Play 已通过 MCP 停止。
- 本节点完成 warning 清理、空引用加固和文档同步。
- 后续建议只保留真实剧情 Selection 分支视觉确认，以及与原版画面的人工对照。
