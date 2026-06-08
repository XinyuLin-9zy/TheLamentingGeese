# 哀鸿 (The Weeping Swan) - 重建项目状态

更新时间：2026-06-02

## 项目概览

- 游戏：《哀鸿》(The Weeping Swan / Starveling)
- 引擎：UTAGE4 v4.2.7 + Unity 2022.3.62f1/f2c1
- 重建来源：AssetRipper 1.3.14 + IL2CPP 反编译资源
- Unity 项目：本仓库根目录。
- Unity MCP：`POST http://127.0.0.1:7891/api/...`

## 当前状态

### 2026-06-02 iOS App Icon / Xcode 导出

- 新增 `Assets/AppIcon/AppIcon-iOS-1024.png`，由项目标题背景裁切为 1024x1024 不透明 PNG。
- `Codex/Build iOS Xcode Project` 现在会自动应用 iOS App Icon，并在 Xcode 工程导出成功后补齐 `ios-marketing` 1024 图标，同时把 appiconset 内 PNG 转为无 alpha。
- 已导出 `Builds/iOS/TheLamentingGeese-iOS`，状态文件显示 `state=succeeded`、`totalErrors=0`、`totalWarnings=5`。
- 已验证 `Unity-iPhone/Images.xcassets/AppIcon.appiconset` 包含 iPhone/iPad 图标和 `Icon-AppStore-1024.png`，所有图标 `hasAlpha: no`。
- Unity MCP `/api/compilation/errors` 返回 0 error；`dotnet build Assembly-CSharp-Editor.csproj --no-restore -v:quiet -clp:ErrorsOnly` 通过。

### 正常

- 编译：`Utage.csproj` 与 `Assembly-CSharp.csproj` 均为 0 error。
- Unity MCP `/api/compilation/errors` 返回 0 error。
- Boot 不再卡在 `isWaitBootLoading`；Title 可以打开，Start 可进入 MainGame。
- 主剧情中文正文已可见，`MessageText (1)` 使用 `NovelTextForTextAnimator`/中文字体路径渲染。
- Save/Load、Config、Gallery、Archive、PlotMap、Selection smoke-test 链路已做运行时兜底与 MCP 复测。
- 主资源目录静态检查无 `m_Script: {fileID: 0}`、无 `m_Content: {fileID: 0}`、无旧 TMP GUID、无内置 Arial `m_Font` 残留。

### 2026-05-19 新增收敛

- `AdvMessageWindowManager.ChangeCurrentWindow` 对旧剧情里的窗口类型 `1/2/3` 静默回退到默认窗口，不再刷 `is not found in window manager` warning。
- `UI_ChapterTitle.prefab` 与 `Assets/Resources/prefab/ui/Particle System.prefab` 的粒子 Shape 从 Sprite 改为 Rectangle，消除 zero surface area warning。
- `UtageUguiMainGame` 和 `UtageUguiSaveLoad` 增加 SaveManager、UiManager、Config、Page、截图相机、存档槽数据等空引用保护。
- `AdvSceneGallerySetting` 的剧情相册 fallback 从 warning 改为普通 log，避免把可接受的降级路径误标为问题。
- 旧备份场景 `level0_backup_import.unity(.meta)` 已移出 `Assets/Scenes`，现在位于 `_external/scene_backups/`，不再污染 Unity 导入和静态扫描。

## 验证记录

- `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`：通过。
- `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`：通过。
- Unity MCP Play smoke：
  - Boot 到 Title 成功。
  - 调用 `UtageUguiTitle.OnTapStart()` 后进入 MainGame。
  - 引擎状态：`waitBoot=False`、`end=False`、`sceneGallery=False`、`ui=Default`。
  - Console 未再出现窗口类型 `1/2/3` warning 或粒子 zero surface area warning。
- 静态扫描主资源根：未发现旧 TMP GUID、Missing Script、空 Content、内置 Arial 字体、关键资源旧粒子 `type: 19`。

## 仍建议人工复核

- 跑到真实剧情 Selection 分支，确认最终视觉位置/字体/动效。
- 如有原版可对照，做一次 Title Spine 与章节标题动画的人工视觉对比。

## 关键资源

- 剧本：`Assets/MonoBehaviour/Starveling.book.asset`
- 场景列表：`Assets/MonoBehaviour/Starveling.scenarios.asset`
- 章节配置：`Assets/MonoBehaviour/Boot.chapter.asset`
- 主场景：`Assets/Scenes/level0.unity`
- UTAGE4 源码：`Assets/Utage/Scripts/`
- 游戏脚本：`Assets/Scripts/Assembly-CSharp/`

## MCP 常用命令

```powershell
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/ping' -ContentType 'application/json' -Body '{}'
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/compilation/errors' -ContentType 'application/json' -Body '{}'
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/console/log' -ContentType 'application/json' -Body '{}'
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:7891/api/editor/play-mode' -ContentType 'application/json' -Body '{"action":"stop"}'
```

## 2026-05-19 Selection/backlog follow-up

- Fixed the live black-screen selection case at `Starveling.book.asset` rowIndex `1151-1156`: the script now fades back in before the `Su Lianyan?` selection instead of leaving `SpriteCamera` `ColorFade` at full black.
- Fixed Backlog display order in `AdvUguiBacklogManager`: backlog data is rendered oldest-to-newest and opens scrolled to the newest entry at the bottom.
- Hardened ending overlays: `CustomCommander.ResolveOverlayCanvas()` restores overlay canvas scale to `Vector3.one`; `ScreenTextCommand` now uses localized/Text-column content while treating `Arg1` as font index when numeric.
- Verification: `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`, `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`, and Unity MCP `/api/compilation/errors` all pass with 0 errors. Current Play session was restored by clearing the stuck runtime `ColorFade`.

## 2026-05-20 Su Lianyan after-choice follow-up

- Fixed the pure black pause after selecting `苏怜烟` / `Su Lianyan?`: the branch correctly played `FadeOut black 2`, `BgOff`, `Jump *忆-第一回`, then chapter animation, but `map_yi_1` immediately added `FadeOut black 0` plus `Wait 2` before loading the next background.
- Changed `Assets/MonoBehaviour/Starveling.book.asset` rowIndex `6` in `map_yi_1` from `Wait 2` to `Wait 0`. The chapter animation and the final `FadeIn black 1` are kept; only the redundant pure-black hold is removed.
- MCP verification from the real branch label rowIndex `1157`: before the change, `MCPShots/su_after_choice_4.6.png` was pure black during `Wait row=6`; after the change, the runtime is already at `AdvCommandFadeIn row=11`, and `MCPShots/su_after_choice_fixed_normal_5.8.png` shows the next scene background.
- Verification: `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`, `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`, and Unity MCP `/api/compilation/errors` all pass with 0 errors.

## 2026-05-20 UI_ChapterTitle sample follow-up

- Compared `sample/UI_ChapterTitle界面_不带粒子界面.png` and `sample/UI_ChapterTitle界面_淡出带粒子界面.png` with the restored chapter-title prefab.
- Confirmed `chapter_img_mask2` is not just a hidden mask; it is the visible dark left overlay and torn vertical edge seen in the sample. `Mask.showMaskGraphic` must stay enabled while `TitleBG` remains its child white brush strip.
- Kept `Assets/GameObject/UI_ChapterTitle.prefab` with corrected UGUI `RectMask2D` / `Mask` script GUIDs, Rectangle particle shape, and `m_ShowMaskGraphic: 1`.
- Verification: `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`, `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`, and Unity MCP `/api/compilation/errors` all pass with 0 errors.

## 2026-05-20 Title screen text follow-up

- Compared `sample/标题界面.png` with `Canvas-AdvUI/Title`. The title menu labels are sprite graphics (`title_button_*`) rather than Unity `Text` components.
- Hardened `UtageUguiTitle` on Awake/Enable/Open so the Title BG stays behind, Spine stays inside BG, Logo/menu buttons/top-right icons/AppVersion stay active and above the BG, language sprite adapters refresh, and title menu `Graphic`/`CanvasRenderer` alpha is restored to visible.
- Corrected the runtime AppVersion fallback back to the top-right anchor used by the sample.
- Verification: `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`, `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`, and Unity MCP `/api/compilation/errors` all pass with 0 errors.

## 2026-05-20 UI sample alignment safe-pause checkpoint

- Safe pause state: Unity Play Mode was stopped before this checkpoint; MCP state was `isPlaying=false`, `isCompiling=false`, active scene `level0`.
- Do not resume the oversized old Codex session from that checkpoint. It previously hit compact 429/524.
- Do not revert the existing dirty `Assets/Scenes/level0.unity`; it is intentionally preserved.
- Title screen progress verified against `sample/标题界面.png`: title Spine now cover-fits the screen, Logo and five bottom menu button sprites are visible, language image adapters refresh, and runtime screenshot was saved at `Assets/Screenshots/codex_title_current_verify.png`.
- Title `记忆回想/记忆回响` behavior was changed to open the Load/SaveLoad UI through `UtageUguiTitle.OnTapArchive()` -> `load.OpenLoad(this)`. Verification screenshot: `Assets/Screenshots/codex_archive_opens_load_verify.png`.
- `良田满穗` / ExtraStory is locked until any common ending key is unlocked. Runtime check showed `UI_ExtraStory.IsUnlocked=False` and button `interactable=False` with no endings cleared. Unlock keys are `map_commonEnd_1` through `map_commonEnd_10`, using any-unlocked logic.
- Main-game extra bottom-right buttons `QSave` and `QLoad` are hidden to match the sample. Runtime check showed `QSave activeSelf=False`, `QLoad activeSelf=False`.
- Money HUD is pinned bottom-left to match the selection sample: `CurMoney` at `(32,32)`, `UseMoney` at `(32,88)`, `CollectVoice` at `(32,144)`. `UI_DialogMsg` disables interfering layout groups and reapplies the layout in `LateUpdate`.
- Chapter title readability still needs one final pass. Current manual screenshots are `Assets/Screenshots/codex_chapter_title_250ms.png` and `Assets/Screenshots/codex_chapter_title_900ms.png`; compare with `sample/UI_ChapterTitle界面_不带粒子界面.png` and `sample/UI_ChapterTitle界面_淡出带粒子界面.png`.
- Final verification still pending after the pause: `dotnet build Utage.csproj --no-restore`, `dotnet build Assembly-CSharp.csproj --no-restore`, and Unity MCP `/api/compilation/errors`.

## 2026-05-21 Chapter title layer safe-pause checkpoint

- Safe pause state: Unity Play Mode was stopped before this checkpoint; MCP state was `isPlaying=false`, `isCompiling=false`, active scene `level0`, `sceneDirty=false`.
- Do not resume the oversized old Codex session from that checkpoint. The parallel investigation agent was still running with no result and was closed.
- Do not revert the existing dirty `Assets/Scenes/level0.unity`; it remains intentionally preserved.
- Latest code fix: `Assets/Scripts/Assembly-CSharp/UI/UI_TitleAnimation.cs` now moves the root visual layer for each chapter-title element to the front. This matters because `TitleBG` is under the root child `Mask`; moving only `TitleBG` left the root `BG` layer able to cover the white brush/title band during the particle title sequence.
- Validation after the latest layer-order patch:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 272 warnings.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.
- Visual recheck still remains for the next resume: enter Play Mode, trigger the chapter title with `title/chapterbg/BG_Ji_1`, `title/titlebg/chapter_bg_J`, `title/textsprite/sc/chapter_title_J1`, `TitleAnimationType.Liang`, offset `265`, and capture the 250ms / 900ms screenshots to confirm the white brush band and title remain readable against the two `sample/UI_ChapterTitle界面_*` images.

## 2026-05-21 Chapter title readability final pass

- Safe pause state after this pass: Unity Play Mode was stopped; MCP state was `isPlaying=false`, `isCompiling=false`, active scene `level0`, `sceneDirty=false`.
- Do not resume the oversized old Codex session from that checkpoint; this pass continued from the handoff summary and parallel agent result only.
- Do not revert the existing dirty `Assets/Scenes/level0.unity`; it remains intentionally preserved.
- Fixed the chapter title text being unreadable throughout the particle/title sequence:
  - `Assets/Shader/Shader Graphs_TitleDisappear.shader`: corrected the dissolve alpha direction so `_Strength=0` keeps the title visible and increasing `_Strength` dissolves it away.
  - `Assets/Scripts/Assembly-CSharp/UI/UI_TitleAnimation.cs`: raised early title/title-brush visibility and reduced the intro black shelter from a full black cover to a light darkening pass.
- Visual recheck against `sample/UI_ChapterTitle界面_不带粒子界面.png` and `sample/UI_ChapterTitle界面_淡出带粒子界面.png`:
  - `Assets/Screenshots/codex_chapter_title_readability_250ms.png`
  - `Assets/Screenshots/codex_chapter_title_readability_550ms.png`
  - `Assets/Screenshots/codex_chapter_title_readability_1200ms.png`
  - `Assets/Screenshots/codex_chapter_title_readability_2000ms.png`
  The 250ms/550ms captures now show the white brush band and title text clearly; 1200ms confirms the title fades instead of being shader-cleared from the start.
- Rechecked the other sample-alignment fixes by screenshot/static references:
  - Title screen screenshot `Assets/Screenshots/codex_title_current_verify.png` remains aligned with `sample/标题界面.png`: Spine covers the screen, logo/menu buttons are visible, and the last title button uses the lock visual.
  - Archive/memory recall opens Load UI: `UtageUguiTitle.OnTapArchive()` calls `load.OpenLoad(this)`, with screenshot `Assets/Screenshots/codex_archive_opens_load_verify.png`.
  - ExtraStory is locked until any common ending key `map_commonEnd_1` through `map_commonEnd_10` is unlocked.
  - Main-game `QSave` and `QLoad` are forced hidden in `UtageUguiMainGame`.
  - Money HUD is pinned bottom-left in `UI_DialogMsg` at `CurMoney (32,32)`, `UseMoney (32,88)`, `CollectVoice (32,144)`.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 272 warnings.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-21 Title Spine size follow-up

- User-provided runtime screenshot showed the title Spine/background art still too small, with the composition not filling the screen.
- Fixed the title Spine fit in `Assets/Utage/Scripts/TemplateUI/UtageUguiTitle.cs` and `Assets/Scripts/Assembly-CSharp/UI_SpineAdaptFitter.cs`:
  - Compute cover scale from the rendered Spine mesh bounds after `UpdateMesh()`.
  - Preserve that cover scale as the base for the title idle wave so the coroutine no longer resets the Spine to scale `1`.
  - Apply a small title-background crop multiplier so the paper/stage edge is cropped like `sample/标题界面.png` instead of remaining visibly framed.
- Runtime verification after forcing Unity `AssetDatabase.Refresh()`:
  - Screenshot saved at `Assets/Screenshots/codex_title_spine_after_refresh.png`.
  - `Canvas-AdvUI/Title/BG/Spine` runtime `localScale` is approximately `(4.875, 4.875, 1)`, rect size is `2265.6 x 1274.4`, mesh bounds are `556.115 x 314.7502`, and `layoutScaleMode=EnvelopeParent`.
  - The title art now fills the 16:9 screen with no black margins, while Logo and the five bottom menu button sprites stay above it.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: passed with 0 errors before runtime screenshot capture.

## 2026-05-21 403 resume UI follow-up

- Migrated an interrupted Codex session from a local Codex session log; that run stopped because child-agent API calls returned HTTP 403 from an internal API endpoint.
- Continued the interrupted UI fixes:
  - Config UI: hides null-texture `RawImage` / `UguiNovelText` white blocks, darkens config controls, keeps language and voice-language values visible, and collapses the default-active resolution template in `IsNotFullScreen`.
  - Save/Load UI: empty save slots now use transparent captures and hide the screenshot placeholder frame/background while preserving the no-data label.
  - Flowchart UI: generated placeholder labels are normalized to unlock-condition text, static connector lines are refreshed, and runtime line elements can position between chapter endpoints.
  - Chapter title animation: the shelter starts transparent, fades without a final black flash, and the canvas stops blocking raycasts when fading/destroyed.
- Runtime verification screenshots:
  - `Assets/Screenshots/codex_settings_whiteblocks_final12.png`
  - `Assets/Screenshots/codex_saveload_empty_final2.png`
  - `Assets/Screenshots/codex_flowchart_lines_final3.png`
  - `Assets/Screenshots/codex_title_anim_450ms.png`
- Runtime checks:
  - Config: `nullWhiteRawImages=0`, `novelRawImagesVisible=0`, `activeTemplates=0`, `languageTexts=2`.
  - Save/Load: `emptyEnabledCapture=0`, `emptyVisiblePic=0`.
  - Flowchart: `placeholders=0`, `staticLines=58`.
  - Title animation: spawned successfully, screenshot at 450ms was not pure black, and the animation instance was destroyed after playback.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 27 warnings.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 349 warnings.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.
  - `git diff --check` on the touched files passed; only Git LF-to-CRLF normalization warnings were reported.

## 2026-05-21 Config/Title resume validation

- Resumed from the Config/Title polish checkpoint and confirmed the runtime helper code is present:
  - `UtageUguiConfig` includes `IsBottomSystemButton`, `EnsureBottomSystemButtonText`, and readable runtime labels for `ButtonEndGame`, `ButtonBackTitle`, and `ButtonBack`.
  - `UtageUguiTitle` and `UI_SpineAdaptFitter` preserve the Spine cover-fit scale while the title idle wave runs.
- Visual spot-checks:
  - `Assets/Screenshots/codex_current_settings_after.png` matches `sample/系统设定界面.png` closely enough for the current pass: null-texture white blocks are gone, language/voice-language labels remain readable, and the bottom system buttons have readable labels.
  - New runtime capture `Assets/Screenshots/codex_resume_current_game.png` confirms the current Title screen still cover-fits the background art with logo/menu/top-right buttons visible.
- Fixed a runtime Console spam case in `Assets/Utage/Scripts/GameLib/Sound/SoundAudio.cs`: `UpdatePlay()` and `IsEndCurrentAudio()` now tolerate missing `Data`, `AudioSource`, or `clip`, ending invalid sound objects instead of repeatedly throwing `NullReferenceException` at `SoundAudio.cs:369`.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 27 warnings.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 349 warnings.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.
  - Latest Unity Console entries after script reload only show MCP startup logs; the repeated `SoundAudio.UpdatePlay()` null reference no longer appears.
  - `git diff --check` on the touched files passed; only Git LF-to-CRLF normalization warnings were reported.

## 2026-05-23 503 resume Float-Light gallery follow-up

- Continued the 503-interrupted session from the recovered handoff without reverting the existing dirty imported assets or scene changes.
- Finished the `浮光掠影` opened-function chain:
  - `AdvVoiceCollectionData` now persists collected backlog voice entries per `AdvEngine`, normalizing by main voice file name and falling back to PlayerPrefs payload storage when needed.
  - `UtageUguiVoiceCollection` and `UtageUguiVoiceCollectionItem` now populate the `语音收藏` list, play collected voices, and support removing entries from the collection.
  - `AdvUguiBacklog` now finds the backlog `collection` control at runtime, toggles collected voice state through reflection, updates the icon state, and shows the collect/uncollect message through `UI_DialogMsg`.
- Finished the locked `良田满穗` behavior:
  - `UI_ExtraStory` keeps the button clickable while locked, uses any common ending key `map_commonEnd_1` through `map_commonEnd_10` as the default unlock condition, and opens a one-button locked-message dialog.
  - `UtageUguiTitle.OnTapExtraStory()` now refreshes `UI_ExtraStory`, detects `IsUnlocked == false`, and invokes `ShowLockedMessage()` instead of silently returning.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 27 warnings.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors and 348 warnings.
  - `git diff --check` on the touched files passed; only Git LF-to-CRLF normalization warnings were reported.
  - Unity MCP runtime recheck was not run because `http://127.0.0.1:7891/api/ping` was unavailable in this session.

## 2026-05-23 Float-Light gallery layout final pass

- Continued the same 503-resumed `浮光掠影` work with Unity MCP available, using parallel read-only agents for visual review, stage-scope review, and voice-collection data review.
- Finished the gallery presentation pass:
  - `UguiLayeredGridPage` now drives the layered CG layout from `GridPage_Layered`, creates runtime page buttons, hides legacy carousel artifacts, and keeps the 4+4 CG grid/page-number rhythm aligned with the sample.
  - `UtageUguiCgGallery` prefers the layered runtime grid and clears both old and layered page contents when reopening.
  - `UtageUguiSoundRoom` and `UtageUguiSoundRoomItem` now normalize the music room to the two-column locked-row layout, with `622x96` cells, `120/27` spacing, and text-only page numbers.
  - `UtageUguiVoiceCollectionItem` uses the same font/title fallback path as the gallery shell so collected voice rows render with readable Chinese text.
- Visual verification:
  - `Assets/Screenshots/codex_floatlight_cg_final_attempt13.png`
  - `Assets/Screenshots/codex_floatlight_music_final_attempt13.png`
  - `Assets/Screenshots/codex_floatlight_voice_final_attempt13.png`
  These were captured after explicitly closing the Title view before opening Gallery, so the earlier bottom title-label ghosting is no longer present.
- Runtime data note:
  - Current runtime/system save reports `cg=95 opened=0`; therefore the clean CG screenshot shows all slots locked. The sample image has the first slot unlocked because it was captured with different gallery save data.
  - Current `AdvVoiceCollectionData` reports `voiceLogs=0`, `voiceWithFile=0`, and empty PlayerPrefs payload; therefore the clean voice screenshot has no collected voice rows. The sample image contains one collected voice entry, which requires runtime collection data rather than a layout change.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 warnings and 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 warnings and 0 errors.
  - Unity MCP `AssetDatabase.Refresh()` completed, then `/api/compilation/errors` returned `count=0`, `isCompiling=false`.

## 2026-05-24 Next UI polish plan

- This is the handoff plan for the next conversation. No implementation has started for these items yet.
- Keep the existing dirty imported assets, scene files, screenshots, and ProjectSettings out of commits unless a specific fix requires an explicit file. Continue committing each completed functional module with `PROJECT_STATUS.md` updated.

### 1. Float-Light CG page switching

- Problem: the page number buttons under `画像鉴赏` appear clickable, but selecting another page does not visibly switch the CG page.
- First checks:
  - Inspect `UguiLayeredGridPage` runtime page-button binding and `UguiToggleGroupIndexed.CurrentIndex` change path.
  - Verify whether the click event reaches the layered grid or only updates the visual toggle.
  - Confirm old carousel/page artifacts are not intercepting pointer events.
- Acceptance:
  - In Play Mode, open `浮光掠影 -> 画像鉴赏`, click several page numbers, and capture screenshots showing different CG slot ranges/page state.
  - Re-run `dotnet build` for `Utage.csproj` and `Assembly-CSharp.csproj`, then Unity MCP `/api/compilation/errors`.

### 2. In-game save/load UI sample match

- Problem: the in-game save and load screens do not match the design references.
- References in `sample/`:
  - `记忆回想界面_读取存档界面.png`
  - `记忆回想界面_覆盖存档界面.png`
- Required behavior:
  - The in-game bottom-right load button must open the correct load/read-save presentation.
  - The in-game bottom-right save button must open the correct overwrite-save presentation.
  - Do not break title-screen archive/load behavior while adjusting the in-game path.
- Likely code areas:
  - `Assets/Utage/Scripts/TemplateUI/UtageUguiMainGame.cs`
  - `Assets/Utage/Scripts/TemplateUI/UtageUguiSaveLoad.cs`
  - `Assets/Utage/Scripts/TemplateUI/UtageUguiSaveLoadItem.cs`
  - Any runtime-only helpers that currently hide empty slot captures or normalize save/load layout.
- Acceptance:
  - Capture both in-game load and save screens after clicking the bottom-right buttons.
  - Compare against the two sample PNGs, including title text, slot layout, empty-slot visuals, overwrite/read mode copy, and button states.

### 3. In-game bottom-right button hover hints

- Problem: when hovering over the in-game bottom-right buttons, a description text should appear directly below the hovered button.
- Reference in `sample/`:
  - `游戏内右下角悬浮在按钮上时的提示.png`
- Required behavior:
  - Add/restore hover hint text for the bottom-right in-game buttons.
  - The hint should appear under the hovered button, hide on pointer exit, and not shift or block the main-game UI.
  - Verify mouse hover behavior through Unity Play Mode, not only static layout.
- Likely code areas:
  - `Assets/Utage/Scripts/TemplateUI/UtageUguiMainGame.cs`
  - Button prefab/runtime binding for the bottom-right command buttons.
- Acceptance:
  - Capture at least one hover screenshot matching the sample's hint placement.
  - Confirm hints disappear when the pointer leaves the button.

### 4. `UI_ChapterTitle` transition/video polish

- Problem: `UI_ChapterTitle` still differs from the video reference, especially black-screen transition timing and small visual effects.
- Reference:
  - `标题视频演示.mp4`
- Required approach:
  - Review the MP4 frame-by-frame before editing; identify the exact black-field timing, title/brush visibility curve, particle timing, fade-out timing, and any lingering frame artifacts.
  - Compare against the current `UI_ChapterTitle` prefab/runtime sequence with timed screenshots or a short capture.
  - Fix the runtime animation rather than only matching a single still frame.
- Likely code/assets:
  - `Assets/Scripts/Assembly-CSharp/UI/UI_TitleAnimation.cs`
  - `Assets/Shader/Shader Graphs_TitleDisappear.shader`
  - `UI_ChapterTitle` prefab and related particle/brush/title assets only if code cannot correct the behavior.
- Acceptance:
  - Capture timed screenshots or video from the current runtime sequence and compare with `标题视频演示.mp4`.
  - Confirm black-field transition no longer flashes or lingers incorrectly, and title/brush/particle timing matches the reference closely enough across the whole sequence.
  - Re-run the two `dotnet build` commands and Unity MCP `/api/compilation/errors`.

## 2026-05-24 Float-Light CG page switching follow-up

- Fixed `浮光掠影 -> 画像鉴赏` page-number clicks not visibly changing pages.
- Root cause: runtime `CarouselButton(Clone)` page toggles were present and programmatic toggle changes rebuilt the layered grid, but the text-only normalization disabled the visible `Image` raycast targets. Real pointer clicks could hit the gallery background instead of the toggle. `UguiToggleGroupIndexed` also reacted to off events, which could make runtime toggle changes noisy.
- Fix:
  - `UguiLayeredGridPage` now creates/normalizes a transparent full-size `ToggleProxy` image for page buttons so EventSystem clicks reach the toggle while keeping the text-only page-number look.
  - `UguiToggleGroupIndexed.OnToggleValueChanged` now ignores toggle-off events and only changes index for the toggle that turns on.
- Play Mode verification:
  - Opened Title -> `浮光掠影` -> `画像鉴赏`.
  - Runtime page buttons: `toggleCount=12`, initial `CurrentPage=0`, `MaxPage=11`.
  - EventSystem raycast at page 4 hit `ToggleProxy`; pointer click changed `CurrentPage` / `CurrentIndex` from `0` to `3`.
  - Next-frame item data changed from page 1 thumbnails (`CG1-1...`, `CG20...`, `CG21...`) to page 4 thumbnails (`cg14`, `cg24`, `cg8`, `cg9`, `cg7`, `cg10`, `cg12`, `cg13`).
  - Screenshots captured for local comparison only and intentionally not staged: `Assets/Screenshots/codex_floatlight_cg_page1_verify.png`, `Assets/Screenshots/codex_floatlight_cg_page4_verify.png`.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-25 UI_ChapterTitle brush black transition follow-up

- Followed up on the black transition mismatch reported after the timing pass. The chapter-title shelter now uses the existing Utage vertical rule textures instead of a plain UI alpha fade.
- `UI_TitleAnimation` resolves `wipe_up` / `wipe_down` from `AdvEffectManager.RuleTextureList`, drives `UguiTransition` on the `Shelter` image, and uses a runtime black 1x1 sprite so the `Utage/UI/RuleFade` shader produces a real black wipe instead of depending on UI tint.
- Non-transition frames disable `UguiTransition` and restore the normal UI material, so the shelter behaves as a normal black overlay during holds and transparent frames.
- Play Mode verification:
  - Triggered `UI_ChapterTitle` with `BG_Ji_1`, `chapter_bg_J`, `chapter_title_J1`, `TitleAnimationType.Liang`, offset `265`, scale `1`.
  - Captured timed screenshots for local comparison only and intentionally not staged: `Assets/Screenshots/codex_title_brush_after_0450ms.png`, `..._0550ms.png`, `..._0610ms.png`, `..._5170ms.png`, `..._5600ms.png`, `..._6220ms.png`, `..._8110ms.png`, `..._8520ms.png`.
  - 0.45s / 0.55s show the initial black cover revealing the chapter frame with a rough edge moving downward; 5.60s shows the fade-to-black covering from the top downward; 6.22s is fully black.
- Unity Console recheck before this pass showed only MCP startup and SceneGallery fallback logs. No save/load read errors, Dropdown/Button setup errors, or title runtime exceptions were present.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-24 In-game Save/Load slot-mode follow-up

- Fixed the in-game bottom-right Save screen using load/read slot graphics even though the root Save/Load state was correct.
- Root cause: `SaveLoadItem.prefab` contains separate `Image_Save` / `Image_Load` / `Image_NoData` slot overlays, but `UtageUguiSaveLoadItem.Refresh(bool isSave)` did not drive those overlays at runtime. Both save and load sub-images could remain active from prefab state, making Save look like Load.
- Fix:
  - `UtageUguiSaveLoadItem` now resolves the slot overlay images at runtime and toggles `Image_Save` only for save mode and `Image_Load` only for load mode.
  - Empty slots keep the no-data overlay and transparent capture behavior, and auto-save rows restore the auto-save background when available.
  - Corrupt/unreadable system, auto, or regular save files are now handled by `TryReadBinaryDecode` call paths: affected slots/system data reset, bad files are removed, and Unity Console no longer receives fatal read-error spam.
  - `IsNotFullScreen` no longer adds a `Button` to a `Dropdown` object that already has a `Selectable`, removing the Console error reported during UI setup.
- Play Mode verification:
  - Title -> Start -> in-game `OnTapSave()` opened `SaveLoad` with `isSave=True`; active slot overlay was `Image_Save` / `sl_data_sub_overwrite`, while `Image_Load` was inactive.
  - In-game `OnTapLoad()` opened `SaveLoad` with `isSave=False`; active slot overlay was `Image_Load` / `sl_data_sub_load`, while `Image_Save` was inactive.
  - Title `OnTapArchive()` still opens `SaveLoad` as load mode (`isSave=False`).
  - Runtime screenshots captured for local comparison only and intentionally not staged: `Assets/Screenshots/codex_saveload_save_slotmode_verify.png`, `Assets/Screenshots/codex_saveload_load_slotmode_verify.png`.
  - Unity Console after verification showed MCP/probe logs only; the earlier save file read errors and Dropdown/Button error did not reappear.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-24 Unity Console lifecycle guard

- Checked Unity Console through MCP after the Save/Load follow-up. The previous save-file read errors and Dropdown/Button setup error were gone, but a script-refresh lifecycle `NullReferenceException` remained in `AssetFileReference.OnDestroy()`.
- Fixed `AssetFileReference.OnDestroy()` so components that are destroyed before `Init()` or during script reload safely skip `Unuse()` when no `AssetFile` has been assigned.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-24 In-game bottom-right hover hints

- Restored the main-game bottom-right button hover hints on the existing `ExplainImage` brush panel.
- `UtageUguiMainGame` now binds runtime `EventTrigger` pointer/select events for the bottom command buttons, keeps the hint graphics from intercepting clicks, positions the hint beside the hovered button while clamping it on-screen, and hides it on pointer exit/deselect.
- Simplified Chinese save/load hover text now matches the sample wording: `存储进度(S)` and `加载进度(D)`.
- Play Mode verification:
  - Title -> Start -> triggered the Save button's actual `PointerEnter` event through MCP.
  - `ExplainImage` became active with text `存储进度(S)` and was captured at `Assets/Screenshots/codex_maingame_save_hover_hint_final.png` for local comparison only.
  - Triggered `PointerExit`; `ExplainImage` became inactive and its text cleared.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-24 UI_ChapterTitle video transition follow-up

- Reviewed `sample/标题视频演示.mp4` frame-by-frame and matched the runtime sequence against the key beats: initial black hold, fast reveal by ~0.61s, particle/title dissolve around ~2.6-4.2s, fade-to-black around ~5.17-6.22s, black hold, then story fade-back around ~8.11-9.12s.
- Fixed the reveal pass so the chapter art is already fully opaque under the black shelter; the shelter now controls the first reveal alone. This removes the earlier 0.61s leak where the underlying story UI/character could show through the chapter title frame.
- Tightened the dissolve pass so the title alpha reaches 0 instead of lingering as a faint ghost, and particles are faded/cleared before the full black frame.
- Kept the particle emitter under the authored `Title` transform so title offset/scale changes do not detach the burst origin from the brush/title composition.
- Runtime verification:
  - Triggered `UI_ChapterTitle` in Play Mode with `BG_Ji_1`, `chapter_bg_J`, `chapter_title_J1`, `TitleAnimationType.Liang`, offset `265`, scale `1`.
  - Captured timed screenshots for local comparison only and intentionally not staged: `Assets/Screenshots/codex_title_anim_followup_0610ms.png`, `..._3000ms.png`, `..._4200ms.png`, `..._5600ms.png`, `..._6220ms.png`, `..._8520ms.png`.
  - 0.61s showed the full chapter frame without story bleed-through; 4.20s showed title dissolve/particle burst; 6.22s was pure black; 8.52s showed the story fading back in.
- Unity Console recheck:
  - No save/load read errors, Dropdown/Button setup errors, `AssetFileReference.OnDestroy()` exceptions, or title runtime exceptions were present after the follow-up probe.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-25 UI_ChapterTitle plain black fade correction

- Corrected the follow-up interpretation after visual review: `UI_ChapterTitle` should not use the rough top-to-bottom rule/brush wipe. That wipe remains reserved for in-game scenario black-screen transitions that pass a non-empty rule texture through the Utage fade command path.
- `UI_TitleAnimation` now drives the chapter-title `Shelter` as a normal black UI image with alpha only. It no longer adds or drives `UguiTransition`, no longer resolves `wipe_up` / `wipe_down`, and clears any runtime shelter material when title info is set.
- Save/Load recheck:
  - MCP Console currently reports `errors=0`, `warnings=0`; no current save/load read errors were present in the last 300 Console entries.
  - Added a null-safe `AdvSaveData.IsSaved` guard so script-refresh / shutdown autosave checks do not throw when a save-data buffer has not been initialized.
- Runtime verification:
  - Triggered `UI_ChapterTitle` in Play Mode with `BG_Ji_1`, `chapter_bg_J`, `chapter_title_J1`, `TitleAnimationType.Liang`, offset `265`, scale `1`.
  - Timed MCP probe covered the full sequence: 0.05s black hold, 0.45s fade reveal, 0.61s fully revealed, 5.60s fade to black, 6.22s full black, 8.10s black hold, and 8.45s return fade.
  - At every sampled point the shelter used `Default UI Material`, had no `UguiTransition`, and changed only normal black alpha (`1.000 -> 0.851 -> 0.000 -> 0.368 -> 1.000 -> 1.000 -> 0.049`).
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.
  - MCP Console after runtime probes: `errors=0`, `warnings=0`.

## 2026-05-25 In-game Save/Load button binding recheck

- Tightened `UtageUguiMainGame` bottom-right command toggle binding so runtime listeners survive re-open/domain-refresh paths and momentary toggles reset without staying selected.
- Added null/active guards around the HideUI and Save paths so inactive main-game views do not open the Save screen during title/archive or closing transitions.
- Play Mode verification:
  - Triggered the actual in-game `Save` toggle; `SaveLoad` opened with `isSave=True`, `Save` root active, `Load` root inactive, and slot 0 showing `Image_Save=True` / `Image_Load=False`.
  - Triggered the actual in-game `Load` toggle; `SaveLoad` opened with `isSave=False`, `Save` root inactive, `Load` root active, and slot 0 showing `Image_Save=False` / `Image_Load=True`.
  - Called title `OnTapArchive()` afterward; it still opened the same SaveLoad view in load mode (`isSave=False`) instead of save mode.
  - MCP Console after the probe still showed only MCP startup / SceneGallery fallback logs, with no save/load read errors or UI setup exceptions.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.

## 2026-05-25 In-game rule-fade black transition

- Restored the `Utage/ImageEffect/RuleFade` shader from the AssetRipper dummy export to an actual rule-texture post effect.
- This keeps `UI_ChapterTitle` on the plain alpha-only black fade from the previous pass, while scenario `FadeOut/FadeIn` commands that specify `wipe_down` / `wipe_up` can use the rough top-to-bottom rule edge in-game.
- Play Mode verification:
  - Started `map_ji_1` at page 9, the transition immediately after the first protagonist line.
  - At ~0.18s, `SpriteCamera` had `RuleFade enabled=True`, `strength=0.550`, `texture=wipe_down`, and no `ColorFade`.
  - At the follow-up fade-in, `SpriteCamera` switched to `texture=wipe_up`; after completion, the `RuleFade` component remained disabled at `strength=0.000`.
  - Captured local comparison screenshots only, intentionally unstaged: `Assets/Screenshots/codex_ingame_rulefade_map_ji_1_0180ms.png` and `Assets/Screenshots/codex_ingame_rulefade_map_ji_1_0610ms.png`.
- Validation:
  - `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly`: passed with 0 errors.
  - Unity MCP `/api/compilation/errors`: 0 errors, `isCompiling=false`.
