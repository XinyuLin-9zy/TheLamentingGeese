// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimura
using UnityEngine;
using UnityEngine.UI;
using Utage;
using UtageExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


namespace Utage
{

	/// <summary>
	/// コンフィグ画面のサンプル
	/// </summary>
	[AddComponentMenu("Utage/TemplateUI/UtageUguiConfig")]
	public class UtageUguiConfig : UguiView
	{
		/// <summary>ADVエンジン</summary>
		public AdvEngine Engine => this.GetAdvEngineCacheFindIfMissing(ref engine);
		[SerializeField] protected AdvEngine engine;

		//コンフィグデータへのインターフェース
		protected virtual AdvConfig Config
		{
			get { return Engine.Config; }
		}
		

		/// <summary>タイトル画面</summary>
		[SerializeField] protected UtageUguiTitle title;

		/// <summary>「フルスクリーン表示」のチェックボックス</summary>
		[SerializeField] protected Toggle checkFullscreen;
		[SerializeField] protected GameObject checkFullscreenRoot;

		/// <summary>「マウスホイールでメッセージ送り」のチェックボックス</summary>
		[SerializeField] protected Toggle checkMouseWheel;
		[SerializeField] protected GameObject checkMouseWheelRoot;

		/// <summary>「未読スキップ」のチェックボックス</summary>
		[SerializeField] protected Toggle checkSkipUnread;

		/// <summary>「選択肢でスキップを解除」チェックボックス</summary>
		[SerializeField] protected Toggle checkStopSkipInSelection;

		/// <summary>「ボイス再生時にメッセージウィンドウを非表示に」チェックボックス</summary>
		[SerializeField] protected Toggle checkHideMessageWindowOnPlyaingVoice;

		/// <summary>「メッセージ速度」のスライダー</summary>
		[SerializeField] protected Slider sliderMessageSpeed;

		/// <summary>「メッセージ速度（既読）」のスライダー</summary>
		[SerializeField] protected Slider sliderMessageSpeedRead;

		/// <summary>「自動メッセージ速度」のスライダー</summary>
		[SerializeField] protected Slider sliderAutoBrPageSpeed;

		/// <summary>「ウインドウの透明度」のスライダー</summary>
		[SerializeField] protected Slider sliderMessageWindowTransparency;

		/// <summary>「サウンド」の音量スライダー</summary>
		[SerializeField] protected Slider sliderSoundMasterVolume;

		/// <summary>「BGM」の音量スライダー</summary>
		[SerializeField] protected Slider sliderBgmVolume;

		/// <summary>「SE」の音量スライダー</summary>
		[SerializeField] protected Slider sliderSeVolume;

		/// <summary>「環境音」の音量スライダー</summary>
		[SerializeField] protected Slider sliderAmbienceVolume;

		/// <summary>「ボイス」の音量スライダー</summary>
		[SerializeField] protected Slider sliderVoiceVolume;

		/// <summary>音声の再生タイプのラジオボタン</summary>
		[SerializeField] protected UguiToggleGroupIndexed radioButtonsVoiceStopType;

		[System.Serializable]
		protected class TagedMasterVolumSliders
		{
			public string tag = "";
			public Slider volumeSlider = null;
		}

		/// <summary>キャラ別のボリューム設定など</summary>
		[SerializeField] protected List<TagedMasterVolumSliders> tagedMasterVolumSliders;

		//確認ダイアログの表示。設定してないときは表示しない
		public SystemUiDialog2Button dialog;


		//開く時にタブをリセットする
		public bool ResetTabIndexOnOpen
		{
			get => resetTabIndexOnOpen;
			set => resetTabIndexOnOpen = value;
		}
		[SerializeField] bool resetTabIndexOnOpen;

		//開く時にタブをリセットする際のタブへの参照
		[SerializeField] UguiToggleGroupIndexed tabToggleGroup;

		[System.NonSerialized] Toggle checkSkipReadOnly;
		[System.NonSerialized] bool runtimeBindingsInitialized;


		//コンフィグの値全体をロードした際に呼ばれるイベント
		//表示開始時や、初期設定に戻したときに呼ばれ、各UIに値を反映された時に呼ばれるので
		//コンフィグUIを拡張して、値を反映する処理を追加する場合は、このイベントを使う
		public UnityEvent OnLoadValues => onLoadValues;
		[SerializeField] UnityEvent onLoadValues = new();
        

		//文字送り速度
		public virtual float MessageSpeed
		{
			set
			{
				if (!IsInit) return;
				Config.MessageSpeed = value;
			}
		}

		//文字送り速度(既読)
		public virtual float MessageSpeedRead
		{
			set
			{
				if (!IsInit) return;
				Config.MessageSpeedRead = value;
			}
		}

		//オート文字送り速度
		public virtual float AutoBrPageSpeed
		{
			set
			{
				if (!IsInit) return;
				Config.AutoBrPageSpeed = value;
			}
		}

		//メッセージウィンドウの透過色（バー）
		public virtual float MessageWindowTransparency
		{
			set
			{
				if (!IsInit) return;
				Config.MessageWindowTransparency = value;
			}
		}

		//音量設定 サウンド全体
		public virtual float SoundMasterVolume
		{
			set
			{
				if (!IsInit) return;
				Config.SoundMasterVolume = value;
			}
		}

		//音量設定 BGM
		public virtual float BgmVolume
		{
			set
			{
				if (!IsInit) return;
				Config.BgmVolume = value;
			}
		}

		//音量設定 SE
		public virtual float SeVolume
		{
			set
			{
				if (!IsInit) return;
				Config.SeVolume = value;
			}
		}

		//音量設定 環境音
		public virtual float AmbienceVolume
		{
			set
			{
				if (!IsInit) return;
				Config.AmbienceVolume = value;
			}
		}

		//音量設定 ボイス
		public virtual float VoiceVolume
		{
			set
			{
				if (!IsInit) return;
				Config.VoiceVolume = value;
			}
		}

		//フルスクリーン切り替え
		public virtual bool IsFullScreen
		{
			set
			{
				if (!IsInit) return;
				Engine.ScreenResolution.IsFullScreen = value;
			}
		}

		//マウスホイールでメッセージ送り切り替え
		public virtual bool IsMouseWheel
		{
			set
			{
				if (!IsInit) return;
				Config.IsMouseWheelSendMessage = value;
			}
		}

		//エフェクトON・OFF切り替え
		public virtual bool IsEffect
		{
			set
			{
				if (!IsInit) return;
				Config.IsEffect = value;
			}
		}

		//未読スキップON・OFF切り替え
		public virtual bool IsSkipUnread
		{
			set
			{
				if (!IsInit) return;
				Config.IsSkipUnread = value;
			}
		}

		//選択肢でスキップ解除ON・OFF切り替え
		public virtual bool IsStopSkipInSelection
		{
			set
			{
				if (!IsInit) return;
				Config.IsStopSkipInSelection = value;
			}
		}

		//ボイス再生時にメッセージウィンドウを非表示にON・OFF切り替え
		public virtual bool HideMessageWindowOnPlyaingVoice
		{
			set
			{
				if (!IsInit) return;
				Config.HideMessageWindowOnPlayingVoice = value;
			}
		}

		public virtual bool IsInit
		{
			get { return isInit; }
			set { isInit = value; }
		}

		protected bool isInit = false;

		/// <summary>
		/// オープンしたときに呼ばれる
		/// </summary>
		protected virtual void OnOpen()
		{
			EnsureRuntimeReferences();
			EnsureRuntimeBindings();
			isInit = false;
			//スクショをクリア
			if (Engine != null && Engine.SaveManager.Type != AdvSaveManager.SaveType.SavePoint)
			{
				Engine.SaveManager.ClearCaptureTexture();
			}

			if (ResetTabIndexOnOpen)
			{
				ResetTabIndex();
			}

			StartCoroutine(CoWaitOpen());
		}

		public void ResetTabIndex()
		{
			if (tabToggleGroup == null)
			{
				Debug.LogWarning("tabToggleGroup is null", this);
				return;
			}
			
			//トグル変更のアニメーションをいったん無効化
			var toggles = tabToggleGroup.TogglesToArray;
			List<Toggle.ToggleTransition> toggleTransitions = new List<Toggle.ToggleTransition>();  
			foreach (var toggle in toggles)
			{
				toggleTransitions.Add(toggle.toggleTransition);
				toggle.toggleTransition = Toggle.ToggleTransition.None;
			}
			//タブインデックスを0にリセット
			tabToggleGroup.CurrentIndex = 0;
			//トグル変更のアニメーションを戻しておく
			for (var i = 0; i < toggles.Length; i++)
			{
				toggles[i].toggleTransition = toggleTransitions[i];
			}
		}


		//起動待ちしてから開く
		protected virtual IEnumerator CoWaitOpen()
		{
			if (Engine == null)
			{
				Debug.LogError("[UtageUguiConfig] AdvEngine is missing. Config UI cannot load values.", this);
				yield break;
			}
			while (Engine.IsWaitBootLoading) yield return null;
			LoadValues();
		}

		/// <summary>
		/// 画面を閉じる処理
		/// </summary>
		public override void Close()
		{
			if (Engine != null)
			{
				Engine.WriteSystemData();
			}
			base.Close();
		}

		protected virtual void Update()
		{
			//右クリックで戻る
			if (isInit && InputUtil.IsInputGuiClose())
			{
				Back();
			}
		}

		//各UIに値を反映
		protected virtual void LoadValues()
		{
			EnsureRuntimeReferences();
			EnsureRuntimeBindings();
			isInit = false;
			if (checkFullscreen) checkFullscreen.isOn = Engine.ScreenResolution.IsFullScreen;
			if (checkMouseWheel) checkMouseWheel.isOn = Config.IsMouseWheelSendMessage;
			if (checkSkipUnread) checkSkipUnread.isOn = Config.IsSkipUnread;
			if (checkSkipReadOnly) checkSkipReadOnly.SetIsOnWithoutNotify(!Config.IsSkipUnread);
			if (checkStopSkipInSelection) checkStopSkipInSelection.isOn = Config.IsStopSkipInSelection;
			if (checkHideMessageWindowOnPlyaingVoice)
				checkHideMessageWindowOnPlyaingVoice.isOn = Config.HideMessageWindowOnPlayingVoice;

			if (sliderMessageSpeed) sliderMessageSpeed.value = Config.MessageSpeed;
			if (sliderMessageSpeedRead) sliderMessageSpeedRead.value = Config.MessageSpeedRead;

			if (sliderAutoBrPageSpeed) sliderAutoBrPageSpeed.value = Config.AutoBrPageSpeed;
			if (sliderMessageWindowTransparency)
				sliderMessageWindowTransparency.value = Config.MessageWindowTransparency;
			if (sliderSoundMasterVolume) sliderSoundMasterVolume.value = Config.SoundMasterVolume;
			if (sliderBgmVolume) sliderBgmVolume.value = Config.BgmVolume;
			if (sliderSeVolume) sliderSeVolume.value = Config.SeVolume;
			if (sliderAmbienceVolume) sliderAmbienceVolume.value = Config.AmbienceVolume;
			if (sliderVoiceVolume) sliderVoiceVolume.value = Config.VoiceVolume;

			if (radioButtonsVoiceStopType) radioButtonsVoiceStopType.CurrentIndex = (int)Config.VoiceStopType;

			//サブマスターボリュームの設定
			foreach (var item in tagedMasterVolumSliders)
			{
				if (string.IsNullOrEmpty(item.tag) || item.volumeSlider == null)
				{
					continue;
				}

				float volume;
				if (Config.TryGetTaggedMasterVolume(item.tag, out volume))
				{
					item.volumeSlider.value = volume;
				}
			}

			//フルスクリーンはPC版のみの操作
			if (!UtageToolKit.IsPlatformStandAloneOrEditor())
			{
				if (checkFullscreen)
				{
					checkFullscreen.gameObject.SetActive(false);
				}
				if(checkFullscreenRoot)
				{
					checkFullscreenRoot.gameObject.SetActive(false);
				}
				//マウスホイールはPC版とWebGL以外では無効
				if (Application.platform != RuntimePlatform.WebGLPlayer)
				{
					if (checkMouseWheel)
					{
						checkMouseWheel.gameObject.SetActive(false);
					}

					if (checkMouseWheelRoot)
					{
						checkMouseWheelRoot.gameObject.SetActive(false);
					}
				}
			}
			
			SanitizeRuntimeVisuals();
			OnLoadValues.Invoke();
			SanitizeRuntimeVisuals();
			StartCoroutine(CoSanitizeRuntimeVisualsDeferred());
			isInit = true;
		}

		protected virtual IEnumerator CoSanitizeRuntimeVisualsDeferred()
		{
			for (int i = 0; i < 3; ++i)
			{
				yield return null;
				SanitizeRuntimeVisuals();
			}
		}

		protected virtual void EnsureRuntimeReferences()
		{
			if (title == null) title = FindSceneObject<UtageUguiTitle>();

			if (checkFullscreen == null) checkFullscreen = FindToggle("FullScreen", "Fullscreen");
			if (checkFullscreenRoot == null && checkFullscreen != null) checkFullscreenRoot = checkFullscreen.gameObject;
			if (checkMouseWheel == null) checkMouseWheel = FindToggle("MouseWheel", "Mouse Wheel");
			if (checkMouseWheelRoot == null && checkMouseWheel != null) checkMouseWheelRoot = checkMouseWheel.gameObject;
			if (checkSkipUnread == null) checkSkipUnread = FindToggle("SkipAll", "Skip All");
			if (checkSkipReadOnly == null) checkSkipReadOnly = FindToggle("SkipRead", "Skip Read");
			if (checkStopSkipInSelection == null) checkStopSkipInSelection = FindToggle("StopSkipInSelection", "StopSkip");
			if (checkHideMessageWindowOnPlyaingVoice == null) checkHideMessageWindowOnPlyaingVoice = FindToggle("Window", "HideMessageWindow", "HideMessageWindowOnPlayingVoice");

			if (sliderMessageSpeed == null) sliderMessageSpeed = FindSlider("MessageSpeed");
			if (sliderMessageSpeedRead == null) sliderMessageSpeedRead = FindSlider("MessageSpeedRead", "ReadMessageSpeed");
			if (sliderAutoBrPageSpeed == null) sliderAutoBrPageSpeed = FindSlider("AutoModeSpeed", "AutoSpeed");
			if (sliderMessageWindowTransparency == null) sliderMessageWindowTransparency = FindSlider("WindowOpacity", "WindowTransparency");
			if (sliderSoundMasterVolume == null) sliderSoundMasterVolume = FindSlider("MasterVolume", "SoundMasterVolume");
			if (sliderBgmVolume == null) sliderBgmVolume = FindSlider("BgmVolume", "BGMVolume");
			if (sliderSeVolume == null) sliderSeVolume = FindSlider("SeVolume", "SEVolume");
			if (sliderAmbienceVolume == null) sliderAmbienceVolume = FindSlider("BGEVolume", "AmbienceVolume");
			if (sliderVoiceVolume == null) sliderVoiceVolume = FindSlider("VoiceVolume", "Voice Volume");

			if (radioButtonsVoiceStopType == null) radioButtonsVoiceStopType = FindComponentByNames<UguiToggleGroupIndexed>("VoiceStop");
			if (tabToggleGroup == null) tabToggleGroup = FindComponentByNames<UguiToggleGroupIndexed>("TabButtons");
		}

		protected virtual void EnsureRuntimeBindings()
		{
			if (runtimeBindingsInitialized) return;

			BindToggle(checkFullscreen, value => IsFullScreen = value);
			BindToggle(checkMouseWheel, value => IsMouseWheel = value);
			BindToggle(checkSkipUnread, value =>
			{
				if (value) IsSkipUnread = true;
			});
			BindToggle(checkSkipReadOnly, value =>
			{
				if (value) IsSkipUnread = false;
			});
			BindToggle(checkStopSkipInSelection, value => IsStopSkipInSelection = value);
			BindToggle(checkHideMessageWindowOnPlyaingVoice, value => HideMessageWindowOnPlyaingVoice = value);

			BindSlider(sliderMessageSpeed, value => MessageSpeed = value);
			BindSlider(sliderMessageSpeedRead, value => MessageSpeedRead = value);
			BindSlider(sliderAutoBrPageSpeed, value => AutoBrPageSpeed = value);
			BindSlider(sliderMessageWindowTransparency, value => MessageWindowTransparency = value);
			BindSlider(sliderSoundMasterVolume, value => SoundMasterVolume = value);
			BindSlider(sliderBgmVolume, value => BgmVolume = value);
			BindSlider(sliderSeVolume, value => SeVolume = value);
			BindSlider(sliderAmbienceVolume, value => AmbienceVolume = value);
			BindSlider(sliderVoiceVolume, value => VoiceVolume = value);

			if (radioButtonsVoiceStopType != null)
			{
				radioButtonsVoiceStopType.OnValueChanged.AddListener(OnTapRadioButtonVoiceStopType);
			}

			BindButton("ButtonBack", Back);
			BindButton("CloseButton", Back);
			BindButton("BtnBack", Back);
			BindButton("Button-Back", Back);
			BindButton("ButtonBackTitle", OnTapBackTitle);
			BindButton("InitButton", OnTapInitDefaultAll);
			BindButton("ResetProcess", OnTapInitDefaultAll);
			BindButton("OpenCharacterVolumeSetting", OnTapOpenCharacterVolumeSetting);

			SanitizeRuntimeVisuals();
			runtimeBindingsInitialized = true;
		}

		protected virtual void SanitizeRuntimeVisuals()
		{
			foreach (Slider slider in GetComponentsInChildren<Slider>(true))
			{
				SanitizeSliderVisuals(slider);
			}

			foreach (Dropdown dropdown in GetComponentsInChildren<Dropdown>(true))
			{
				SanitizeDropdownVisuals(dropdown);
			}

			foreach (Toggle toggle in GetComponentsInChildren<Toggle>(true))
			{
				SanitizeToggleVisuals(toggle);
			}

			foreach (Scrollbar scrollbar in GetComponentsInChildren<Scrollbar>(true))
			{
				SanitizeScrollbarVisuals(scrollbar);
			}

			foreach (Image image in GetComponentsInChildren<Image>(true))
			{
				SanitizeLooseBuiltInImage(image);
			}

			foreach (RawImage rawImage in GetComponentsInChildren<RawImage>(true))
			{
				SanitizeLooseBuiltInRawImage(rawImage);
			}

			foreach (Selectable selectable in GetComponentsInChildren<Selectable>(true))
			{
				SanitizeSelectableVisuals(selectable);
			}
		}

		protected virtual void SanitizeSelectableVisuals(Selectable selectable)
		{
			if (selectable == null) return;

			Image targetImage = selectable.targetGraphic as Image;
			if (targetImage != null && IsConfigButtonOrControlBlock(targetImage))
			{
				StyleConfigControlBlock(targetImage);
				StyleSelectableColorBlock(selectable, targetImage.color);
				return;
			}

			RawImage targetRawImage = selectable.targetGraphic as RawImage;
			if (IsNullTextureLayoutRawImage(targetRawImage))
			{
				SetTransparentLayoutRawImage(targetRawImage, true);
				StyleTransparentSelectableColorBlock(selectable);
			}
		}

		protected virtual void SanitizeSliderVisuals(Slider slider)
		{
			if (slider == null) return;

			Transform background = FindChildRecursive(slider.transform, "Background");
			if (background != null)
			{
				Image backgroundImage = background.GetComponent<Image>();
				StyleSliderBar(backgroundImage, new Color(0.02f, 0.02f, 0.02f, 0.92f), 5f);
			}

			if (slider.fillRect != null)
			{
				Image fillImage = slider.fillRect.GetComponent<Image>();
				StyleSliderBar(fillImage, new Color(0.86f, 0.86f, 0.86f, 0.88f), 5f);
			}

			if (slider.handleRect != null)
			{
				Image handleImage = slider.handleRect.GetComponent<Image>();
				if (handleImage != null)
				{
					handleImage.color = Color.white;
					handleImage.raycastTarget = true;
					handleImage.type = Image.Type.Simple;
				}

				RectTransform handleRect = slider.handleRect;
				Vector2 size = handleRect.sizeDelta;
				handleRect.sizeDelta = new Vector2(Mathf.Max(size.x, 20f), Mathf.Max(size.y, 20f));
				handleRect.localScale = Vector3.one;
			}
		}

		protected virtual void SanitizeDropdownVisuals(Dropdown dropdown)
		{
			if (dropdown == null) return;

			if (dropdown.targetGraphic is Image targetImage && IsLikelyBuiltInBlock(targetImage))
			{
				SetTransparentLayoutImage(targetImage);
			}

			if (dropdown.captionText != null)
			{
				dropdown.captionText.color = new Color(0.08f, 0.08f, 0.09f, 1f);
			}

			if (dropdown.itemText != null)
			{
				dropdown.itemText.color = new Color(0.08f, 0.08f, 0.09f, 1f);
			}

			if (dropdown.template != null)
			{
				foreach (Image image in dropdown.template.GetComponentsInChildren<Image>(true))
				{
					if (image == null) continue;
					if (NameEquals(image.transform, "Viewport")
					    || NameEquals(image.transform, "Item Background")
					    || NameEquals(image.transform, "Item Checkmark"))
					{
						SetTransparentLayoutImage(image);
					}
				}
			}

			Transform arrow = FindChildRecursive(dropdown.transform, "Arrow");
			if (arrow != null)
			{
				Image arrowImage = arrow.GetComponent<Image>();
				if (arrowImage != null)
				{
					arrowImage.color = new Color(0.92f, 0.92f, 0.94f, 1f);
					arrowImage.raycastTarget = false;
				}
			}
		}

		protected virtual void SanitizeToggleVisuals(Toggle toggle)
		{
			if (toggle == null) return;

			if (toggle.targetGraphic is Image backgroundImage && IsLikelyBuiltInBlock(backgroundImage))
			{
				SetTransparentLayoutImage(backgroundImage);
			}

			if (toggle.graphic is Image checkImage)
			{
				checkImage.color = new Color(0.92f, 0.92f, 0.94f, checkImage.color.a);
				checkImage.raycastTarget = false;
			}
		}

		protected virtual void SanitizeScrollbarVisuals(Scrollbar scrollbar)
		{
			if (scrollbar == null) return;

			if (scrollbar.targetGraphic is Image targetImage && IsLikelyBuiltInBlock(targetImage))
			{
				targetImage.color = new Color(0.88f, 0.88f, 0.9f, 0.82f);
				targetImage.raycastTarget = true;
			}

			if (scrollbar.handleRect != null)
			{
				Image handleImage = scrollbar.handleRect.GetComponent<Image>() ?? scrollbar.handleRect.GetComponentInChildren<Image>(true);
				if (handleImage != null && IsLikelyBuiltInBlock(handleImage))
				{
					handleImage.color = new Color(0.94f, 0.94f, 0.95f, 1f);
					handleImage.raycastTarget = true;
					handleImage.type = Image.Type.Simple;
				}
			}
		}

		protected virtual void SanitizeLooseBuiltInImage(Image image)
		{
			if (image == null) return;
			if (IsDropdownTransparentImage(image))
			{
				SetTransparentLayoutImage(image);
				return;
			}
			if (IsConfigButtonOrControlBlock(image))
			{
				StyleConfigControlBlock(image);
				return;
			}
			if (!IsLikelyBuiltInBlock(image)) return;
			if (image.GetComponentInParent<Slider>(true) != null) return;
			if (image.GetComponentInParent<Scrollbar>(true) != null) return;
			if (image.transform == transform) return;

			Selectable selectable = image.GetComponent<Selectable>();
			if (selectable != null && selectable.targetGraphic == image) return;

			Color color = image.color;
			if (color.a <= 0.001f) return;
			if (color.r < 0.72f || color.g < 0.72f || color.b < 0.72f) return;

			SetTransparentLayoutImage(image);
		}

		protected virtual void SanitizeLooseBuiltInRawImage(RawImage rawImage)
		{
			if (!IsNullTextureLayoutRawImage(rawImage)) return;

			Selectable selectable = rawImage.GetComponent<Selectable>();
			bool isNovelTextOverlay = rawImage.GetComponent<UguiNovelText>() != null;
			bool keepRaycastTarget = !isNovelTextOverlay && selectable != null && selectable.targetGraphic == rawImage;
			SetTransparentLayoutRawImage(rawImage, keepRaycastTarget);
			if (selectable != null)
			{
				StyleTransparentSelectableColorBlock(selectable);
			}
		}

		protected virtual bool IsNullTextureLayoutRawImage(RawImage rawImage)
		{
			if (rawImage == null) return false;
			if (rawImage.texture != null) return false;
			if (rawImage.transform == transform) return false;
			if (rawImage.GetComponent<UguiNovelText>() != null) return true;
			if (rawImage.GetComponentInParent<Slider>(true) != null) return false;
			if (rawImage.GetComponentInParent<Scrollbar>(true) != null) return false;

			Color color = rawImage.color;
			if (color.a <= 0.001f) return false;
			return color.r >= 0.72f && color.g >= 0.72f && color.b >= 0.72f;
		}

		protected virtual bool IsConfigButtonOrControlBlock(Image image)
		{
			if (image == null || image.sprite == null) return false;

			string spriteName = image.sprite.name ?? "";
			if (NameContains(spriteName, "sys_language_bg")) return true;
			if (NameContains(spriteName, "sys_language_button_")) return true;
			if (NameContains(spriteName, "sys_button_")) return true;
			if (NameContains(spriteName, "sys_resolution_")) return true;
			if (NameContains(spriteName, "dialog_menu_explaintext_bg")) return true;
			return false;
		}

		protected virtual void StyleConfigControlBlock(Image image)
		{
			if (image == null) return;

			string spriteName = image.sprite != null ? image.sprite.name ?? "" : "";
			bool isBottomSystemButton = IsBottomSystemButton(image.transform);
			bool isPillControl = NameContains(spriteName, "sys_language_bg")
				|| NameContains(spriteName, "sys_resolution_")
				|| NameContains(spriteName, "sys_button_normal");

			image.color = Color.white;

			if (image.GetComponent<Selectable>() == null)
			{
				image.raycastTarget = false;
			}

			StyleControlTexts(image.transform, isPillControl ? new Color(0.08f, 0.08f, 0.09f, 1f) : Color.white);
			if (isBottomSystemButton)
			{
				StyleBottomSystemButton(image);
			}
		}

		protected virtual bool IsBottomSystemButton(Transform target)
		{
			if (target == null) return false;

			return NameEquals(target, "ButtonEndGame")
				|| NameEquals(target, "ButtonBackTitle")
				|| NameEquals(target, "ButtonBack");
		}

		protected virtual void StyleBottomSystemButton(Image image)
		{
			if (image == null) return;

			string label = GetBottomSystemButtonLabel(image.transform);
			if (string.IsNullOrEmpty(label)) return;

			Text text = EnsureBottomSystemButtonText(image.transform, label);
			if (text == null) return;

			text.text = label;
			text.color = Color.white;
			text.raycastTarget = false;
			text.alignment = TextAnchor.MiddleCenter;
			text.fontStyle = FontStyle.Bold;
			text.fontSize = NameEquals(image.transform, "ButtonBack") ? 31 : 27;
			text.resizeTextForBestFit = true;
			text.resizeTextMinSize = 18;
			text.resizeTextMaxSize = text.fontSize;
			text.horizontalOverflow = HorizontalWrapMode.Overflow;
			text.verticalOverflow = VerticalWrapMode.Overflow;

			Outline outline = text.GetComponent<Outline>() ?? text.gameObject.AddComponent<Outline>();
			outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
			outline.effectDistance = new Vector2(1.25f, -1.25f);
			outline.useGraphicAlpha = true;
		}

		protected virtual string GetBottomSystemButtonLabel(Transform target)
		{
			if (NameEquals(target, "ButtonEndGame")) return "结束游戏";
			if (NameEquals(target, "ButtonBackTitle")) return "回到标题";
			if (NameEquals(target, "ButtonBack")) return "返回";
			return null;
		}

		protected virtual Text EnsureBottomSystemButtonText(Transform root, string label)
		{
			if (root == null) return null;

			Transform child = FindDirectChild(root, "RuntimeReadableLabel");
			GameObject labelObject = child != null ? child.gameObject : new GameObject("RuntimeReadableLabel", typeof(RectTransform));
			if (child == null)
			{
				labelObject.transform.SetParent(root, false);
			}

			RectTransform rectTransform = labelObject.transform as RectTransform;
			if (rectTransform != null)
			{
				rectTransform.anchorMin = Vector2.zero;
				rectTransform.anchorMax = Vector2.one;
				rectTransform.pivot = new Vector2(0.5f, 0.5f);
				rectTransform.offsetMin = Vector2.zero;
				rectTransform.offsetMax = Vector2.zero;
				rectTransform.localScale = Vector3.one;
				rectTransform.localRotation = Quaternion.identity;
			}

			Text text = labelObject.GetComponent<Text>() ?? labelObject.AddComponent<Text>();
			if (text.font == null)
			{
				Text template = FindTemplateText(root);
				text.font = template != null && template.font != null
					? template.font
					: Resources.GetBuiltinResource<Font>("Arial.ttf");
			}
			return text;
		}

		protected virtual Text FindTemplateText(Transform excludedRoot)
		{
			foreach (Text text in GetComponentsInChildren<Text>(true))
			{
				if (text == null) continue;
				if (excludedRoot != null && text.transform.IsChildOf(excludedRoot)) continue;
				if (text.font != null) return text;
			}
			return null;
		}

		protected virtual void StyleSelectableColorBlock(Selectable selectable, Color normal)
		{
			if (selectable == null) return;

			ColorBlock colors = selectable.colors;
			colors.normalColor = normal;
			colors.highlightedColor = new Color(0.94f, 0.94f, 0.97f, Mathf.Max(normal.a, 1f));
			colors.selectedColor = colors.highlightedColor;
			colors.pressedColor = new Color(0.78f, 0.78f, 0.88f, Mathf.Max(normal.a, 1f));
			colors.disabledColor = new Color(0.68f, 0.67f, 0.82f, Mathf.Max(normal.a, 1f));
			colors.colorMultiplier = 1f;
			colors.fadeDuration = 0.06f;
			selectable.colors = colors;
		}

		protected virtual void StyleTransparentSelectableColorBlock(Selectable selectable)
		{
			if (selectable == null) return;

			Color transparent = new Color(1f, 1f, 1f, 0f);
			ColorBlock colors = selectable.colors;
			colors.normalColor = transparent;
			colors.highlightedColor = transparent;
			colors.selectedColor = transparent;
			colors.pressedColor = transparent;
			colors.disabledColor = transparent;
			colors.colorMultiplier = 1f;
			colors.fadeDuration = 0.02f;
			selectable.colors = colors;
		}

		protected virtual void StyleControlTexts(Transform root)
		{
			StyleControlTexts(root, new Color(0.96f, 0.96f, 0.98f, 1f));
		}

		protected virtual void StyleControlTexts(Transform root, Color color)
		{
			if (root == null) return;

			foreach (Text text in root.GetComponentsInChildren<Text>(true))
			{
				if (text == null) continue;
				text.color = color;
				text.raycastTarget = false;
			}

			foreach (TMPro.TMP_Text text in root.GetComponentsInChildren<TMPro.TMP_Text>(true))
			{
				if (text == null) continue;
				text.color = color;
				text.raycastTarget = false;
			}
		}

		protected virtual void StyleSliderBar(Image image, Color color, float height)
		{
			if (image == null) return;

			image.color = color;
			image.raycastTarget = false;
			image.type = Image.Type.Simple;
			CompressHorizontalRect(image.rectTransform, height);
		}

		protected virtual void SetTransparentLayoutImage(Image image)
		{
			if (image == null) return;

			Color color = image.color;
			color.a = 0f;
			image.color = color;
			image.raycastTarget = false;
		}

		protected virtual void SetTransparentLayoutRawImage(RawImage rawImage, bool keepRaycastTarget)
		{
			if (rawImage == null) return;

			Color color = rawImage.color;
			color.a = 0f;
			rawImage.color = color;
			rawImage.raycastTarget = keepRaycastTarget;
		}

		protected virtual void CompressHorizontalRect(RectTransform rectTransform, float height)
		{
			if (rectTransform == null) return;

			rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, 0.5f);
			rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, 0.5f);
			rectTransform.pivot = new Vector2(rectTransform.pivot.x, 0.5f);
			rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			rectTransform.localScale = Vector3.one;
		}

		protected virtual bool NameEquals(Transform target, string name)
		{
			return target != null && string.Equals(target.name, name, System.StringComparison.OrdinalIgnoreCase);
		}

		protected virtual Transform FindDirectChild(Transform root, string name)
		{
			if (root == null) return null;

			foreach (Transform child in root)
			{
				if (NameEquals(child, name)) return child;
			}
			return null;
		}

		protected virtual bool IsLikelyBuiltInBlock(Image image)
		{
			if (image == null) return false;

			string imageName = image.name ?? "";
			string spriteName = image.sprite != null ? image.sprite.name ?? "" : "";
			if (NameContains(imageName, "Background") || NameContains(imageName, "Checkmark") || NameContains(imageName, "Handle")) return true;
			if (NameContains(imageName, "Viewport") && NameContains(spriteName, "UIMask")) return true;
			if (NameContains(spriteName, "Knob") && image.GetComponentInParent<Selectable>(true) != null) return true;
			if (NameContains(imageName, "Image") && (image.sprite == null || NameContains(spriteName, "UISprite"))) return true;
			if (NameContains(spriteName, "UISprite") || NameContains(spriteName, "Background") || NameContains(spriteName, "InputFieldBackground")) return true;
			return image.sprite == null;
		}

		protected virtual bool IsDropdownTransparentImage(Image image)
		{
			if (image == null) return false;

			string imageName = image.name ?? "";
			string spriteName = image.sprite != null ? image.sprite.name ?? "" : "";
			return (NameContains(imageName, "Viewport") && NameContains(spriteName, "UIMask"))
				|| (NameContains(imageName, "Item Checkmark") && NameContains(spriteName, "Knob"));
		}

		protected virtual bool NameContains(string value, string fragment)
		{
			return !string.IsNullOrEmpty(value)
				&& value.IndexOf(fragment, System.StringComparison.OrdinalIgnoreCase) >= 0;
		}

		protected virtual void BindToggle(Toggle toggle, UnityAction<bool> action)
		{
			if (toggle == null || action == null) return;
			toggle.onValueChanged.AddListener(action);
		}

		protected virtual void BindSlider(Slider slider, UnityAction<float> action)
		{
			if (slider == null || action == null) return;
			slider.onValueChanged.AddListener(action);
		}

		protected virtual void BindButton(string buttonName, UnityAction action)
		{
			Transform target = FindChildRecursive(transform, buttonName);
			if (target == null || action == null) return;

			Button button = target.GetComponent<Button>();
			if (button == null)
			{
				button = target.gameObject.AddComponent<Button>();
			}
			if (button.targetGraphic == null)
			{
				button.targetGraphic = target.GetComponent<Graphic>() ?? target.GetComponentInChildren<Graphic>(true);
			}

			button.onClick.RemoveListener(action);
			button.onClick.AddListener(action);
		}

		protected virtual Toggle FindToggle(params string[] names)
		{
			return FindComponentByNames<Toggle>(names);
		}

		protected virtual Slider FindSlider(params string[] names)
		{
			return FindComponentByNames<Slider>(names);
		}

		protected virtual T FindComponentByNames<T>(params string[] names) where T : Component
		{
			foreach (string name in names)
			{
				Transform target = FindChildRecursive(transform, name);
				if (target == null) continue;

				T component = target.GetComponent<T>();
				if (component != null) return component;
				component = target.GetComponentInChildren<T>(true);
				if (component != null) return component;
			}
			return null;
		}

		protected virtual T FindSceneObject<T>() where T : Component
		{
			foreach (T item in Resources.FindObjectsOfTypeAll<T>())
			{
				if (item != null && item.gameObject.scene.IsValid())
				{
					return item;
				}
			}
			return null;
		}

		protected static Transform FindChildRecursive(Transform root, string name)
		{
			if (root == null) return null;
			if (root.name == name) return root;

			foreach (Transform child in root)
			{
				Transform found = FindChildRecursive(child, name);
				if (found != null) return found;
			}
			return null;
		}

		//タイトルに戻る
		public virtual void OnTapBackTitle()
		{
			void BackTitle()
			{
				Engine.EndScenario();
				this.Close();
				title.Open();
			}
			if (dialog!=null)
			{
				dialog.OpenYesNo(LanguageSystemText.LocalizeText(SystemText.UtageDialogMessageBackTitleConfirm),
					BackTitle, () => { });
			}
			else
			{
				BackTitle();
			}
		}

		//全てデフォルト値で初期化
		public virtual void OnTapInitDefaultAll()
		{
			if (!IsInit) return;

			void InitDefaultAll()
			{
				Config.InitDefaultAll();
				LoadValues();
			}

			if (dialog != null)
			{
				dialog.OpenYesNo(LanguageSystemText.LocalizeText(SystemText.UtageDialogMessageResetConfigConfirm),
					InitDefaultAll, () => { });
			}
			else
			{
				InitDefaultAll();
			}
		}

		public virtual void OnTapOpenCharacterVolumeSetting()
		{
			Transform target = FindChildRecursive(transform, "CharacterVolume");
			if (target == null) return;

			target.gameObject.SetActive(true);
			target.SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
		}

		//音声設定（クリックで停止、次の音声まで再生を続ける）
		public virtual void OnTapRadioButtonVoiceStopType(int index)
		{
			if (!IsInit) return;
			Config.VoiceStopType = (VoiceStopType)index;
		}

		//タグつきボリュームの設定
		public virtual void OnValugeChangedTaggedMasterVolume(string tag, float value)
		{
			if (!IsInit) return;
			Config.SetTaggedMasterVolume(tag, value);
		}
	}
}
