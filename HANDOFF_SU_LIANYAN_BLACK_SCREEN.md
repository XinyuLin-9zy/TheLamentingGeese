# 苏连烟选项后黑屏交接

更新时间：2026-05-20

## 当前用户反馈

用户已经在 Unity Play 中点了“苏连烟”选项。点选之后画面会黑屏一段时间，但 Scene 场景层级里能看到数据存在。

这个现象不要按“场景未加载”优先处理。更可能是画面被黑色 Fade/遮罩/CanvasGroup/相机 ColorFade 覆盖，或者剧本分支在 `FadeOut`、`BgOff`、`Jump` 后没有及时 `FadeIn` 或显示下一段背景。

## 之前已经确认过的点

- 黑屏卡在选项前时，不是引擎崩溃：`AdvEngine` 仍在运行，处于等待选择状态。
- 当时选项 UI 对象存在，选项文本也存在。
- 选项前黑屏的直接原因是 `SpriteCamera` 上的 `Utage.ColorFade` 仍为黑色全强度。
- 临时在运行时禁用该 `ColorFade` 后，选项可见。
- 已在 `Assets/MonoBehaviour/Starveling.book.asset` 的该选项前插入过 `FadeIn black 1`，用于修复“选项出现前仍黑屏”的问题。

## 与该位置相关的已改剧本区段

文件：

`Assets\MonoBehaviour\Starveling.book.asset`

已知相关行附近：

- rowIndex `1151`：已有 `FadeOut black 3`
- rowIndex `1154`：新增 `FadeIn black 1`
- rowIndex `1155`：`Selection`，选项为 `苏怜烟` / `Su Lianyan?`
- rowIndex `1156`：该选项分支 label
- rowIndex `1157`：`StopAmbience`
- rowIndex `1158`：`StopSe`
- rowIndex `1159`：`FadeOut black 2`
- rowIndex `1160`：`BgOff`
- rowIndex `1161`：`Jump`

用户现在说的是点选“苏连烟”之后的黑屏，所以重点应从 rowIndex `1156` 之后，以及 `Jump` 目标位置开始查。

## 当前最可能的问题

高概率是分支后执行了：

1. `FadeOut black 2`
2. `BgOff`
3. `Jump`

但跳转目标后没有及时执行有效的 `Bg` / `BgOn` / `FadeIn` / 窗口显示恢复，导致场景对象或数据已经存在，但 Game 画面仍被黑色 Fade 层盖住。

也可能是跳转目标确实在加载下一段场景，但某个等待条件、演出命令、遮罩命令没有结束，使黑屏停留时间异常变长。

## 2026-05-20 实际定位与修复

已用 Unity MCP 复现并确认：点选 `苏怜烟` 后没有残留 `SpriteCamera` / `UICamera` `ColorFade`，也不是引擎停在等待选择。实际顺序是：

1. `苏怜烟` label 执行 rowIndex `1159`：`FadeOut black 2`。
2. rowIndex `1160`：`BgOff`。
3. rowIndex `1161`：`Jump *忆-第一回`。
4. `忆-第一回` rowIndex `1`：`ChapterAnimation`，正常播放约 2.9 秒。
5. 自动进入 `map_yi_1` 后，rowIndex `5` 又执行 `FadeOut black 0`，rowIndex `6` 原本执行 `Wait 2`，这就是用户看到的额外纯黑停顿。
6. rowIndex `8` 加载 `Bg 静听`，rowIndex `11` 执行 `FadeIn black 1`。

已将 `Assets/MonoBehaviour/Starveling.book.asset` 中 `map_yi_1` rowIndex `6` 的 `Wait` 参数从 `2` 改为 `0`。保留章节动画和最终 `FadeIn black 1`，只移除冗余黑场等待。

验证记录：

- 旧截图 `MCPShots/su_after_choice_4.6.png`：纯黑，运行态停在 `map_yi_1` 的 `Wait row=6`。
- 修改后同一路径：4.6 秒时已进入 `AdvCommandFadeIn row=11`，5.8 秒截图 `MCPShots/su_after_choice_fixed_normal_5.8.png` 可见下一场景背景。
- `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`：0 error。
- `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`：0 error。
- Unity MCP `/api/compilation/errors`：0 error。

## 接手后建议先做的验证

不要先大范围改剧本。先在用户当前黑屏点用 Unity MCP 读运行态：

- 当前 `AdvEngine` 的 page / label / scenario / 是否等待输入。
- 当前正在执行或刚执行完的命令。
- `SpriteCamera`、主相机、UI 相机上的 `ColorFade` 是否 enabled，强度是否仍为 1。
- 是否存在黑色全屏 Image、CanvasGroup alpha=1 的遮罩、或全屏 Panel 盖在最上层。
- 背景 Sprite / Character / TextWindow 是否 active，alpha 是否为 0。
- `Jump` 目标 label 后的前 20-40 行命令，是否缺失 `FadeIn black ...`。

常用 MCP 入口：

```powershell
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/editor/state' -ContentType 'application/json' -Body '{}'
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/compilation/errors' -ContentType 'application/json' -Body '{}'
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/console/log' -ContentType 'application/json' -Body '{}'
```

截图：

```powershell
$body = @{ path = 'MCPShots/su_lianyan_after_choice_black.png'; superSize = 1 } | ConvertTo-Json -Compress
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/screenshot/game' -ContentType 'application/json' -Body $body
```

## 可能的修复方向

如果确认黑屏时 `ColorFade` 仍为黑色全强度，并且跳转目标已经加载了可显示内容，则在跳转目标进入可见内容前补一条合适的 `FadeIn black 1` 或恢复显示层。

如果确认是 `BgOff` 后跳转到一段没有背景恢复的文本，则需要在目标段补背景命令或移除不该出现的 `BgOff`。

如果确认是等待某个演出命令结束，则查对应自定义命令的 `Wait` 条件，尤其是画面外动画、音频停止、标题/ScreenText/Staff overlay 这类命令。

## 这次不要重复排查的内容

- 选项前黑屏已经定位过：是 `SpriteCamera` 的 `ColorFade` 未恢复。
- 当前新问题是点选“苏连烟”之后的黑屏，排查范围应从该分支和跳转目标开始。
- Scene 里有数据这一点很关键，说明优先查显示层、相机 Fade、遮罩和剧本淡入淡出顺序。

## 其他本轮已动过的相关文件

这些改动已编译通过，但仍建议完整 Play 验证：

- `Assets/Utage/Scripts/ADV/UI/AdvUguiBacklogManager.cs`
  - 调整 Backlog 数据顺序为旧到新，并尝试打开时滚到底部。
- `Assets/Scripts/Assembly-CSharp/CustomCommander.cs`
  - 恢复 overlay canvas 的 `localScale = Vector3.one`，避免结尾遮罩/标题 canvas 因 scale=0 不可见。
- `Assets/Scripts/Assembly-CSharp/ScreenTextCommand.cs`
  - 优先使用本地化/Text 列文本，数字 `Arg1` 作为字体索引处理。

已跑过：

```powershell
dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly
dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly
```

两者当时均为 0 errors，Unity MCP `/api/compilation/errors` 当时也为 0 errors。
