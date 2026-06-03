# The Weeping Swan - Architecture Context

This file is a compact MCP-facing summary. The full handoff document is `ARCHITECTURE.md` at the project root.

## System Overview

- Unity 2022.3.62f2c1 visual novel project based on UTAGE4 v4.2.7.
- Internal project code name: `Starveling`.
- Core story data lives under `Assets/MonoBehaviour/`:
  - `Starveling.scenarios.asset`
  - `Boot.chapter.asset`
  - `Starveling.book.asset`
- UTAGE4 source is present under `Assets/Utage/Scripts/`.
- Game-specific scripts live mostly under `Assets/Scripts/Assembly-CSharp/`.

## Boot Flow

1. `AdvEngineStarter.Awake()` calls `LoadEngineAsync()`.
2. `AdvEngine.BootFromExportData(scenarios, "Starveling")` initializes UTAGE subsystems.
3. `UtageUguiBoot.CoUpdate()` waits until `Engine.IsWaitBootLoading == false`.
4. Title UI opens, then Start/Load/Config route through `UtageUguiTitle`.
5. Main-game menu actions route through `UtageUguiMainGame`.

## Current Repair State

As of 2026-05-19:

- `dotnet build .\Utage.csproj --no-restore -v:quiet` passes.
- `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet` passes.
- Unity MCP works at `POST http://127.0.0.1:7891/api/...`; the old GET-style notes are obsolete.
- Unity MCP `/api/compilation/errors` reports 0 errors.
- Project main asset roots (`Assets/GameObject`, `Assets/Resources`, `Assets/Scenes`) have no remaining `m_Script: {fileID: 0}`.
- Project main asset roots have no remaining `m_Content: {fileID: 0}`.
- Project main asset roots have no remaining built-in Arial `m_Font` references.
- Remaining Arial references were fixed in:
  - `Assets/Resources/Accessibility Manager.prefab`
  - `Assets/GameObject/DebugLogItem.prefab`
  - `Assets/Resources/UAP Virtual Keyboard.prefab`
- The old bad script GUIDs documented in root `ARCHITECTURE.md` problem 2b have no remaining occurrences in the project main asset roots.
- `UI_ChapterTitle.prefab` root now resolves to `RectMask2D`, not `GridLayoutGroup`; Unity loads it with `LayoutGroups=0`.
- Title Spine has a valid `SkeletonGraphic.skeletonDataAsset`, `startingAnimation: idle`, and scene Canvas `m_AdditionalShaderChannelsFlag: 25`.
- `AdvMessageWindowManager.ChangeCurrentWindow()` no longer warns for legacy window names `1/2/3`; it falls back to the default window.
- `UtageUguiMainGame` and `UtageUguiSaveLoad` now guard the runtime paths that were prone to null references after reconstruction.
- `UI_ChapterTitle.prefab` and the unused template particle prefab now use Rectangle Shape, so the zero-surface-area particle warning is gone.
- The old backup scene has been moved out of `Assets/Scenes` into `_external/scene_backups/`, so it no longer pollutes static scans.
- MCP Play smoke test confirms:
  - `AdvEngine.IsWaitBootLoading == false` after boot.
  - Title Spine is active, initialized, using `1_SkeletonData`, and playing `idle`.
  - Load opens from Title with 5 active slots and 5 slot buttons.
  - Empty-slot Load shows runtime `__RuntimeGuideMessage` and logs no error.
  - Existing saved slot loads into MainGame at `map_ji_1`.
  - Save opens from MainGame with 5 active slots and 5 slot buttons.
  - Selection smoke test creates 1 option, binds a Button, and clears cleanly after click.
- Console has 0 errors across the verified paths; no `LayoutRebuilder.PerformLayoutCalculation` errors.
- Title/Gallery/PlotMap repair was verified on 2026-05-13:
  - Title `Gallery` opens normally.
  - Gallery `VoiceCollection` tab is bound through a runtime `EventTrigger`; triggering it activates VoiceCollection and closes Cg/Sound views.
  - Title `Archive` opens Gallery's `SceneGallery` view and closes VoiceCollection.
  - Title `PlotMap` opens `UI_PlotMap` and calls `ShowMap(bool)` through reflection instead of Unity `SendMessage`.
  - `UI_PlotMap` creates 65 visible runtime chapter labels in unlocked state.
  - `UI_PlotMap` horizontal scrollbar has a handle and moves Content from x `0` to `-12080` when value changes from `0` to `1`.
  - The stray `Arrow` Scrollbar under PlotMap Content is disabled so it cannot steal drag events.
  - Console error count is 0 after the smoke test.
- Title Spine visual scale was repaired on 2026-05-14:
  - Root cause: `UtageUguiTitle.EnsureSpineTitleBackground()` forced `localScale = 5` and tried to set `MatchRectTransformWithBounds` as a property even though Spine exposes it as a method.
  - Runtime now initializes `SkeletonGraphic`, resets layout scale fields, calls `MatchRectTransformWithBounds()`, anchors the RectTransform full-screen with `localScale = 1`, sets `layoutScaleMode = EnvelopeParent`, and disables `raycastTarget`.
  - MCP Play verification: Spine active, `layoutScaleMode = EnvelopeParent`, `referenceSize = (9230, 5224)`, mesh size `1920 x 1086.68` over a `1920 x 1080` Title BG, no new console error/exception.
- Title Spine asset-preview material was repaired on 2026-05-14:
  - `1_Atlas.asset` uses `Assets/Material/1_Material.mat`; that material previously referenced reconstructed `Assets/Shader/Spine_Skeleton.shader`, a dummy opaque shader with no Spine PMA blending.
  - `1_Material.mat` now references spine-unity's standard `Assets/Spine/Runtime/spine-unity/Shaders/Spine-Skeleton.shader`.
  - Unity verification: material shader path is the spine-unity shader, main texture is `1_0`, `_StraightAlphaInput = 0`, matching `1.atlas.txt` `pma:true`.
- Title Spine runtime UI material was repaired on 2026-05-14:
  - The scene `SkeletonGraphic.m_Material` uses `Assets/Material/SkeletonGraphicDefault.mat`, which previously referenced reconstructed `Assets/Shader/Spine_SkeletonGraphic.shader`, another dummy opaque shader.
  - `SkeletonGraphicDefault.mat` now references spine-unity's standard `Assets/Spine/Runtime/spine-unity/Shaders/SkeletonGraphic/Spine-SkeletonGraphic.shader`.
  - `UtageUguiTitle.OnOpen()` now also runs deferred Spine fitting on the next frame and at end-of-frame because SpineGraphic can write the RectTransform back to `100x100` after its first initialization pass.
  - Cold Play verification: RectTransform uses an overscanned cover size of `2265.60 x 1274.40`, mesh is `2265.60 x 1282.29`, and both `SkeletonGraphic.material` and `CanvasRenderer.GetMaterial()` use the standard SkeletonGraphic shader.
- `UI_ChapterTitle` background cover behavior was tightened on 2026-05-14:
  - `UI_TitleAnimation.SetInfo()` now cover-fits only the drifting `BG`; `TitleBG` has been restored to the prefab's parent-stretched layout under `Mask`.
  - The previous runtime pass over-inflated `TitleBG` with `bgOffset/scale` and produced `2514x1414.13` on a `1920x1080` parent, which broke the prefab's internal proportions.
  - Safety padding is now applied through a uniform scale factor. A previous pass added padding independently to width and height, turning `1920x1080` into `2240x1400` and visibly distorting the aspect ratio.
  - `FitImageToParentCover()` now resolves parent size through stretched RectTransform ancestors, because immediately after `SetInfo()` stretches the root, Unity can still report the prefab's stale `100x100` rect in the same frame.
  - Final title-size root cause: `UI_ChapterTitle.prefab` root had a RectMask2D-shaped component serialized with old GUID `8a8695521f0d02e499659fee002a26c2`; in the current `com.unity.ugui@1.0.0`, that GUID is `GridLayoutGroup`, while `RectMask2D` is `3312d7739989d2b4e91e6319e9a96d76`.
  - The misresolved `GridLayoutGroup` forced root children into `100x100` grid cells at runtime, corrupting `BG/Mask/Title` internal proportions even when the prefab YAML looked correct.
  - `UI_ChapterTitle.prefab`, `UI_Staff.prefab`, and `UI_ScreenText.prefab` now use the correct RectMask2D GUID.
  - `UI_TitleAnimation.Init()` disables any unexpected root `LayoutGroup` as a runtime guard.
  - `Title` text keeps the prefab's original position, size, scale, and `preserveAspect=false`, then applies only additive offsets/scales. This prevents both top-left collapse and the too-small title caused by forced `preserveAspect=true`.
  - `CustomCommander` now serializes `TitleAnimationScaleData`, and `level0.unity` rebinds it to `Assets/MonoBehaviour/TitleAnimationScaleData.asset`.
  - `TitleAnimationCommand` now multiplies the script command scale by the per-title/per-language rate from `TitleAnimationScaleData`, so title size comes from the original restored data instead of a one-off hardcoded guess.
  - 2026-05-20 sample comparison confirmed `Mask` must keep `showMaskGraphic = true`: `chapter_img_mask2` is the visible dark left overlay and torn vertical edge, while its child `TitleBG` supplies the white brush title strip. Hiding the mask graphic removes the sample's dark framing.
  - Current compile verification: `dotnet build .\Assembly-CSharp.csproj --no-restore -v:quiet -clp:ErrorsOnly` passes; `dotnet build .\Utage.csproj --no-restore -v:quiet -clp:ErrorsOnly` passes.
  - Current Unity verification: AssetDatabase loads `UI_ChapterTitle` root as `RectMask2D` with `LayoutGroups=0`; Play `SetInfo(..., bgOffset=265, scale=1)` reports `BG=1984x1116`, `Mask=1920x1080`, `TitleBG=1920x1080`, and `Title pos=(1088,0) size=(336,77) scale=(1.99) preserveAspect=false`.
- Gallery thumbnail loading was hardened on 2026-05-14:
  - Root cause: `AdvUguiLoadGraphicFile.LoadTextureFile()` started its texture wait coroutine on the thumbnail GameObject itself; Gallery can initialize `ThumainalImage` while it is inactive.
  - Fix: inactive targets now use a hidden `CoroutineRunner`, while request ids prevent stale loads from writing back after `ClearFile()` or a newer load request.
  - `dotnet build` passes for both `Utage.csproj` and `Assembly-CSharp.csproj`; final Play click verification for Gallery is still recommended.
- Main story text rendering was repaired on 2026-05-13:
  - Old TMP asset script GUID `67dfb1fdfb2b407222eda8e23ac8b724` has no remaining `Assets/**/*.asset` or `Assets/**/*.unity` occurrences.
  - `SourceHanSerifCN-Bold SDF.asset` loads as `TMPro.TMP_FontAsset` with 5397 glyphs.
  - TMP default settings now use `SourceHanSerifCN-Bold SDF` instead of LiberationSans.
  - `AdvUguiMessageWindowManager` moves itself after `DialogMsg` at runtime so message text is not covered by the bottom UI overlay.
  - `AdvUguiMessageWindow` keeps the scene's legacy `NovelTextForTextAnimator` path when `textPro` is null, disables unused TMP components on the same body text object, and syncs the legacy body font size from the TMP component's configured 32 px size.
  - Fresh MCP Play verification from Title Start shows visible Chinese main text; runtime body text is `SourceHanSerifCN-Regular`, font size 32, TMP component disabled, and `MessageWindowManager` sibling is after `DialogMsg`.
- Selection/backlog follow-up was repaired on 2026-05-19:
  - Root cause of the live black screen at label `记-第一回选项后` page 275: scenario rowIndex `1151` did `FadeOut black 3` and then entered `Selection` without a matching `FadeIn`, leaving `SpriteCamera` `ColorFade` at full black while the selection UI existed underneath.
  - `Assets/MonoBehaviour/Starveling.book.asset` now inserts `FadeIn black 1` before the `Su Lianyan?` selection row so the choice is visible on future runs.
  - `AdvUguiBacklogManager` now renders `BacklogManager.Backlogs[index]` in natural oldest-to-newest order and opens with `verticalNormalizedPosition = 0`, putting the latest line at the bottom.
  - `CustomCommander.ResolveOverlayCanvas()` restores overlay canvas scale to `Vector3.one`; `ScreenTextCommand` reads localized/Text-column content and treats numeric `Arg1` as font index, preventing ending ScreenText/Staff overlays from running invisibly on a zero-scale canvas.
  - Verification: both `dotnet build` commands and Unity MCP `/api/compilation/errors` pass with 0 errors. Current Play session was restored in-place by clearing the stuck runtime `ColorFade`; the player remains on the visible one-option selection.
- Su Lianyan after-choice black pause was repaired on 2026-05-20:
  - MCP reproduction showed the branch did not leave `SpriteCamera` or `UICamera` `ColorFade` stuck. It executed `FadeOut black 2`, `BgOff`, `Jump *忆-第一回`, the chapter animation, then entered `map_yi_1`.
  - The extra pure black hold came from `map_yi_1` rowIndex `6`, where `Wait 2` ran immediately after `FadeOut black 0` and before `Bg 静听`.
  - `Assets/MonoBehaviour/Starveling.book.asset` rowIndex `6` in `map_yi_1` is now `Wait 0`; the chapter animation and final `FadeIn black 1` are preserved.
  - Verification: the old 4.6s screenshot was pure black during `Wait row=6`; after the change, the runtime is already at `AdvCommandFadeIn row=11`, and the 5.8s screenshot shows the next scene background. Both `dotnet build` commands and Unity MCP `/api/compilation/errors` pass with 0 errors.

## Runtime Fallbacks

- `UtageUguiMainGame` binds Save/Load/Config/Auto/Skip/History/HideUI/QSave/QLoad/PlotMap at runtime.
- `UtageUguiTitle` binds Archive/PlotMap/ExtraStory/Exit at runtime and routes PlotMap via reflection to the one-argument `ShowMap(bool)`.
- `UtageUguiTitle` restores the legacy Title visual stack on open: BG/Spine behind, Logo/menu sprite buttons/top-right icons/AppVersion above, language image/button adapters refreshed, and menu `Graphic`/`CanvasRenderer` alpha forced visible.
- `UtageUguiGallery` repairs missing view mappings, binds non-standard Gallery tabs with `EventTrigger`, supports `OpenNamedView(..., "SceneGallery")`, and handles `VoiceCollection` as a plain GameObject view.
- `UI_PlotMap` repairs ScrollRect/Scrollbar/Text references, disables stray Scrollbars, and refreshes PlotMap progress at runtime.
- `UI_PlotChapterElement` creates `__RuntimeChapterLabel` under unlocked panels so flowchart node text renders even when the original Lock panel text is inactive.
- Title Spine uses a runtime bounds/EnvelopeParent fitting fallback to cover the Title BG without the previous forced scale/crop.
- `1_Material.mat` uses the standard spine-unity `Spine/Skeleton` shader so `1_SkeletonData`/atlas previews use proper PMA transparency instead of the reconstructed opaque dummy shader.
- `SkeletonGraphicDefault.mat` uses the standard spine-unity `Spine/SkeletonGraphic` shader so the in-game UI Spine path uses proper UI/PMA rendering.
- `UI_TitleAnimation` now cover-fits the animated chapter `BG` with aspect-preserving padding against its own motion offsets while leaving `TitleBG` in the prefab's stretched mask relationship.
- `UI_TitleAnimation` preserves the prefab `Title` RectTransform baseline, size, scale, and `preserveAspect=false`; it only applies additive offsets/scales and disables unexpected root `LayoutGroup`s.
- `UI_ChapterTitle.prefab`, `UI_Staff.prefab`, and `UI_ScreenText.prefab` have their RectMask2D components remapped from the old GridLayoutGroup GUID to the current RectMask2D GUID.
- `CustomCommander.titleAnimationScaleData` and `TitleAnimationCommand` restore the original per-title language scaling table for `chapter_title_*` sprites.
- `AdvUguiLoadGraphicFile` can load Gallery/SceneGallery/SaveLoad thumbnails even when the target thumbnail object is inactive, by delegating the wait coroutine to a hidden runner.
- `UtageUguiSaveLoad`, `UtageUguiSaveLoadItem`, and `UguiGridPage` recover missing Save/Load/Gallery UI references at runtime.
- `UguiGridPage` creates `__RuntimeGrid` when a reconstructed object already has another `LayoutGroup` and cannot receive a `GridLayoutGroup`.
- `UtageUguiSaveLoad` creates a runtime `__RuntimeGuideMessage` when `guideMessage` is missing and uses safe fallback text when `SystemText` localization rows are absent.
- `AdvUguiSelectionManager`, `AdvUguiSelection`, and `UguiListView` recover delayed/missing Selection references and content roots at runtime.
- `AdvUguiSelection` now uses plain `Text` + Chinese OS font fallback when its text reference is missing, avoiding dynamic `UguiNovelText` generator null refs.
- `AdvUguiBacklogManager` restores natural chronological history order and opens at the latest entry at the bottom.
- `CustomCommander.ResolveOverlayCanvas()` forces overlay roots active, enabled, and `localScale = Vector3.one` before creating ScreenText/Staff overlays.
- `UtageUguiTitle.EnsureSpineTitleBackground()` forces the Title Spine object active, calls `SkeletonGraphic.Initialize(false)` through reflection, calls `MatchRectTransformWithBounds()`, and applies full-screen `EnvelopeParent` fitting.
- Config/UI controls have runtime binding fallbacks for fullscreen, skip mode, voice stop mode, language switching, and character volume.
- `AdvUguiMessageWindowManager` and `AdvUguiMessageWindow` have runtime fallbacks for reconstructed message-window hierarchy/order and mixed legacy/TMP text components.

## Still Needs Play Verification

- After a full manual route from normal gameplay, revisit label `记-第一回选项后` and confirm the `Su Lianyan?` choice and the transition into `map_yi_1` feel correct to the eye. MCP branch-level verification has passed.
- Title Spine: fixed by runtime bounds/EnvelopeParent fitting; remaining manual work is only a final visual eyeball pass against the original game if available.
